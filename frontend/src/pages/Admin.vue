<template>
  <div class="page"><Header /><main class="admin">
    <section class="hero"><div><p class="eyebrow">Панель администратора</p><h1>Пациенты и назначения</h1><p>Создавайте отдельные назначения с дедлайном и сохраняйте историю прохождений.</p></div>
      <div class="stats"><div><strong>{{ users.length }}</strong><span>пациентов</span></div><div><strong>{{ activeCount }}</strong><span>активных</span></div><div><strong>{{ overdueCount }}</strong><span>просрочено</span></div><div><strong>{{ completedCount }}</strong><span>завершено</span></div></div>
    </section>
    <div v-if="loading" class="message">Загрузка...</div>
    <template v-else>
      <p v-if="error" class="message error">{{ error }}</p>
      <form class="panel patient-panel" @submit.prevent="createUser">
        <header><h2>Новый пациент</h2><p>Обязательны только ник, пароль, возраст и пол. Остальные данные можно добавить позже.</p></header>
        <div class="patient-grid">
          <label class="field"><span>Ник</span><input v-model.trim="patientForm.username" minlength="3" maxlength="100" autocomplete="off" required /></label>
          <label class="field"><span>Временный пароль</span><input v-model="patientForm.password" type="password" minlength="12" maxlength="128" autocomplete="new-password" required /></label>
          <label class="field"><span>Повторите пароль</span><input v-model="patientForm.password_confirmation" type="password" minlength="12" maxlength="128" autocomplete="new-password" required /></label>
          <label class="field"><span>Возраст</span><input v-model.number="patientForm.age" type="number" min="0" max="120" required /></label>
          <label class="field"><span>Пол</span><select v-model="patientForm.gender" required><option value="">Выберите</option><option value="F">Женский</option><option value="M">Мужской</option></select></label>
          <label class="field"><span>Email (необязательно)</span><input v-model.trim="patientForm.email" type="email" /></label>
          <label class="field"><span>Телефон (необязательно)</span><input v-model.trim="patientForm.phone" type="tel" /></label>
          <label class="field"><span>Дата рождения (необязательно)</span><input v-model="patientForm.birth_date" type="date" /></label>
          <label class="field"><span>Место жительства (необязательно)</span><input v-model.trim="patientForm.residence" /></label>
        </div>
        <button class="primary" :disabled="savingUser || !canCreateUser">{{ savingUser ? 'Создаём...' : 'Создать пациента' }}</button>
      </form>
      <section class="panel antigen-panel">
        <header><h2>Реестр антигенов</h2><p>У пациента может быть несколько антигенов, а один антиген можно назначить нескольким пациентам.</p></header>
        <form class="antigen-create" @submit.prevent="createAntigen">
          <label class="field"><span>Название антигена</span><input v-model.trim="antigenForm.name" maxlength="100" placeholder="Например, HLA-B27" required /></label>
          <button class="primary" :disabled="savingAntigen || !antigenForm.name">{{ savingAntigen ? 'Создаём...' : 'Добавить антиген' }}</button>
        </form>
        <div class="selector-grid">
          <label class="field"><span>Поиск пациента</span><input v-model.trim="antigenPatientSearch" type="search" placeholder="Ник, email или UUID" /></label>
          <label class="field"><span>Пациент</span><select v-model="antigenUserId"><option value="">Выберите пациента</option><option v-for="u in antigenUserOptions" :key="u.id" :value="u.id">{{ userLabel(u) }}</option></select></label>
        </div>
        <div v-if="selectedAntigenUser" class="antigen-editor">
          <div class="editor-head"><div><strong>{{ selectedAntigenUser.username }}</strong><small>Выбрано: {{ selectedAntigenUser.antigen_ids.length }}</small></div><label class="field compact"><span>Поиск антигена</span><input v-model.trim="antigenSearch" type="search" placeholder="Название или UUID" /></label></div>
          <fieldset class="antigen-picker"><legend>Антигены</legend><label v-for="antigen in filteredAntigens" :key="antigen.id" class="check"><input v-model="selectedAntigenUser.antigen_ids" type="checkbox" :value="antigen.id" /><span><strong>{{ antigen.name }}</strong><small>{{ antigen.id }} · назначен {{ antigen.assigned_user_count }} пациентам</small></span></label><span v-if="!filteredAntigens.length" class="empty">Антигены не найдены.</span></fieldset>
          <button type="button" class="primary" @click="assignAntigens">Сохранить антигены пациента</button>
        </div>
        <p v-else class="empty">Найдите и выберите пациента для назначения.</p>
      </section>
      <section v-if="incompleteUsers.length" class="panel incomplete-panel">
        <header><h2>Нужно дополнить данные — {{ incompleteUsers.length }}</h2><p>Этим пациентам нельзя назначать или проходить опросники, пока не заполнены возраст и пол.</p></header>
        <div class="selector-grid">
          <label class="field"><span>Поиск пациента</span><input v-model.trim="incompletePatientSearch" type="search" placeholder="Ник, email или UUID" /></label>
          <label class="field"><span>Пациент</span><select v-model="incompleteUserId"><option value="">Выберите пациента</option><option v-for="u in incompleteUserOptions" :key="u.id" :value="u.id">{{ userLabel(u) }}</option></select></label>
        </div>
        <article v-if="selectedIncompleteUser" class="incomplete-row">
          <strong>{{ selectedIncompleteUser.username }}</strong>
          <label class="field"><span>Возраст</span><input v-model.number="selectedIncompleteUser.age" type="number" min="0" max="120" /></label>
          <label class="field"><span>Пол</span><select v-model="selectedIncompleteUser.gender"><option value="">Выберите</option><option value="F">Женский</option><option value="M">Мужской</option></select></label>
          <button type="button" class="primary" :disabled="!Number.isInteger(selectedIncompleteUser.age) || !selectedIncompleteUser.gender" @click="saveDemographics(selectedIncompleteUser)">Сохранить</button>
        </article>
      </section>
      <section class="workspace">
        <form class="panel" @submit.prevent="assignQuestionnaires"><header><h2>Новое назначение</h2><p>Можно выбрать несколько опросников.</p></header>
          <label class="field"><span>Пациент</span><select v-model="assignmentForm.user_id" required><option value="">Выберите пациента</option><option v-for="u in users" :key="u.id" :value="u.id">{{ userLabel(u) }}</option></select></label>
          <fieldset class="picker"><legend>Опросники</legend><label v-for="s in systems" :key="s.id" class="check"><input v-model="assignmentForm.system_ids" type="checkbox" :value="s.id" /><span><strong>{{ s.name }}</strong><small>{{ s.question_count }} вопросов</small></span></label></fieldset>
          <div class="row"><label class="field"><span>Срок</span><input v-model="assignmentForm.due_at" type="datetime-local" /></label><label class="field"><span>Приоритет</span><select v-model="assignmentForm.priority"><option value="low">Низкий</option><option value="normal">Обычный</option><option value="high">Высокий</option><option value="urgent">Срочный</option></select></label></div>
          <label class="field"><span>Комментарий пациенту</span><textarea v-model.trim="assignmentForm.note" rows="3" placeholder="Цель назначения или инструкция"></textarea></label>
          <button class="primary" :disabled="saving || !canAssign">{{ saving ? 'Назначаем...' : `Назначить (${assignmentForm.system_ids.length})` }}</button>
        </form>
        <form class="panel" @submit.prevent="createAppointment"><header><h2>Новый приём</h2><p>Дата, специалист и примечание.</p></header>
          <label class="field"><span>Пациент</span><select v-model="appointmentForm.user_id" required><option value="">Выберите пациента</option><option v-for="u in users" :key="u.id" :value="u.id">{{ userLabel(u) }}</option></select></label>
          <div class="row"><label class="field"><span>Дата</span><input v-model="appointmentForm.date" type="date" required /></label><label class="field"><span>Время</span><input v-model="appointmentForm.time" type="time" required /></label></div>
          <div class="row"><label class="field"><span>Специализация</span><input v-model.trim="appointmentForm.specialist" /></label><label class="field"><span>Врач</span><input v-model.trim="appointmentForm.doctor" /></label></div>
          <label class="field"><span>Комментарий</span><textarea v-model.trim="appointmentForm.comment" rows="3"></textarea></label><button class="primary" :disabled="savingAppointment">Назначить приём</button>
        </form>
      </section>
      <p v-if="success" class="message success">{{ success }}</p>
      <section class="panel assignments"><div class="toolbar"><header><h2>Назначения</h2><p>{{ filteredAssignments.length }} записей</p></header><input v-model.trim="search" type="search" placeholder="Пациент или опросник" /><select v-model="statusFilter"><option value="">Все статусы</option><option value="pending">Ожидает</option><option value="in_progress">В процессе</option><option value="completed">Завершён</option><option value="cancelled">Отменён</option></select></div>
        <div class="list"><article v-for="a in filteredAssignments" :key="a.id" class="assignment"><div><strong>{{ a.username }}</strong><small>{{ a.email || 'без email' }}</small><p>{{ a.name }}</p></div><div class="meta"><span :class="['tag', `priority-${a.priority}`]">{{ priorityLabel(a.priority) }}</span><span>{{ a.due_at ? `до ${formatDate(a.due_at)}` : 'без срока' }}</span><span>{{ a.answer_count }} ответов · {{ a.skipped_count }} пропусков</span></div><div class="actions"><span :class="['tag', `status-${a.status}`]">{{ statusLabel(a.status) }}</span><button v-if="['pending','in_progress'].includes(a.status)" type="button" class="cancel" @click="cancelAssignment(a)">Отменить</button></div></article><p v-if="!filteredAssignments.length" class="empty">Назначений не найдено.</p></div>
      </section>
    </template>
  </main></div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from 'vue'
import api from '@/api/axios'
import Header from '@/components/Header.vue'
const users=ref([]),antigens=ref([]),systems=ref([]),assignments=ref([]),loading=ref(true),error=ref(''),success=ref(''),saving=ref(false),savingUser=ref(false),savingAntigen=ref(false),savingAppointment=ref(false),search=ref(''),statusFilter=ref('')
const antigenPatientSearch=ref(''),antigenUserId=ref(''),antigenSearch=ref(''),incompletePatientSearch=ref(''),incompleteUserId=ref('')
const patientForm=reactive({username:'',password:'',password_confirmation:'',age:null,gender:'',email:'',phone:'',birth_date:'',residence:''})
const antigenForm=reactive({name:''})
const assignmentForm=reactive({user_id:'',system_ids:[],due_at:'',priority:'normal',note:''})
const appointmentForm=reactive({user_id:'',date:'',time:'',specialist:'',doctor:'',comment:''})
const activeCount=computed(()=>assignments.value.filter(a=>['pending','in_progress'].includes(a.status)).length)
const completedCount=computed(()=>assignments.value.filter(a=>a.status==='completed').length)
const overdueCount=computed(()=>assignments.value.filter(a=>['pending','in_progress'].includes(a.status)&&a.due_at&&new Date(a.due_at)<new Date()).length)
const incompleteUsers=computed(()=>users.value.filter(u=>!u.profile_complete))
const selectedAntigenUser=computed(()=>users.value.find(u=>u.id===antigenUserId.value)||null)
const selectedIncompleteUser=computed(()=>incompleteUsers.value.find(u=>u.id===incompleteUserId.value)||null)
const matchesUser=(u,query)=>!query||`${u.username} ${u.email||''} ${u.id}`.toLowerCase().includes(query.toLowerCase())
const userOptions=(source,query,selectedId)=>{const options=source.filter(u=>matchesUser(u,query)).slice(0,100),selected=source.find(u=>u.id===selectedId);if(selected&&!options.some(u=>u.id===selected.id))options.unshift(selected);return options}
const antigenUserOptions=computed(()=>userOptions(users.value,antigenPatientSearch.value,antigenUserId.value))
const incompleteUserOptions=computed(()=>userOptions(incompleteUsers.value,incompletePatientSearch.value,incompleteUserId.value))
const filteredAntigens=computed(()=>{const query=antigenSearch.value.toLowerCase();return antigens.value.filter(a=>!query||`${a.name} ${a.id}`.toLowerCase().includes(query)).slice(0,200)})
const canAssign=computed(()=>assignmentForm.user_id&&assignmentForm.system_ids.length)
const canCreateUser=computed(()=>patientForm.password.length>=12&&patientForm.password===patientForm.password_confirmation)
const filteredAssignments=computed(()=>assignments.value.filter(a=>(!statusFilter.value||a.status===statusFilter.value)&&(!search.value||`${a.username} ${a.email||''} ${a.name}`.toLowerCase().includes(search.value.toLowerCase()))))
onMounted(load)
const normalizeUser=u=>({...u,antigens:Array.isArray(u.antigens)?u.antigens:[],antigen_ids:Array.isArray(u.antigen_ids)?u.antigen_ids:[]})
async function load(){loading.value=true;error.value='';try{const [u,g,s,a]=await Promise.all([api.get('/api/admin/users'),api.get('/api/admin/antigens'),api.get('/api/admin/systems'),api.get('/api/admin/questionnaire-assignments')]);users.value=u.data.map(normalizeUser);antigens.value=g.data;systems.value=s.data;assignments.value=a.data}catch(e){error.value=e?.response?.data?.error||'Не удалось загрузить админку'}finally{loading.value=false}}
async function refresh(){assignments.value=(await api.get('/api/admin/questionnaire-assignments')).data}
async function createUser(){error.value='';success.value='';if(patientForm.password!==patientForm.password_confirmation){error.value='Пароли не совпадают';return}savingUser.value=true;try{const payload={username:patientForm.username,password:patientForm.password,age:patientForm.age,gender:patientForm.gender,email:patientForm.email||null,phone:patientForm.phone||null,birth_date:patientForm.birth_date||null,residence:patientForm.residence||null};const {data}=await api.post('/api/admin/users',payload);users.value.push(normalizeUser(data));users.value.sort((a,b)=>a.username.localeCompare(b.username,'ru'));success.value=`Пациент ${data.username} создан`;Object.assign(patientForm,{username:'',password:'',password_confirmation:'',age:null,gender:'',email:'',phone:'',birth_date:'',residence:''})}catch(e){error.value=e?.response?.data?.error||'Не удалось создать пациента'}finally{savingUser.value=false}}
async function createAntigen(){error.value='';success.value='';savingAntigen.value=true;try{const {data}=await api.post('/api/admin/antigens',{name:antigenForm.name});antigens.value.push(data);antigens.value.sort((a,b)=>a.name.localeCompare(b.name,'ru'));success.value=`Антиген ${data.name} создан: ${data.id}`;antigenForm.name=''}catch(e){error.value=e?.response?.data?.error||'Не удалось создать антиген'}finally{savingAntigen.value=false}}
async function assignAntigens(){const user=selectedAntigenUser.value;if(!user)return;error.value='';success.value='';try{const {data}=await api.patch(`/api/admin/users/${user.id}/antigens`,{antigen_ids:user.antigen_ids});Object.assign(user,normalizeUser(data));antigens.value=(await api.get('/api/admin/antigens')).data;success.value=data.antigens.length?`Антигены пациента ${user.username} обновлены`:`У пациента ${user.username} сняты все антигены`}catch(e){error.value=e?.response?.data?.error||'Не удалось назначить антигены';const current=(await api.get('/api/admin/users')).data.find(item=>item.id===user.id);if(current)Object.assign(user,normalizeUser(current))}}
async function saveDemographics(user){error.value='';success.value='';try{const {data}=await api.patch(`/api/admin/users/${user.id}/demographics`,{age:user.age,gender:user.gender});Object.assign(user,data);incompleteUserId.value='';success.value=`Данные пациента ${user.username} обновлены`}catch(e){error.value=e?.response?.data?.error||'Не удалось обновить данные пациента'}}
async function assignQuestionnaires(){saving.value=true;error.value='';success.value='';try{const {data}=await api.post('/api/admin/questionnaire-assignments',{...assignmentForm,due_at:assignmentForm.due_at||null,note:assignmentForm.note||null});success.value=`Создано назначений: ${data.assignments.length}`;assignmentForm.system_ids=[];assignmentForm.note='';assignmentForm.due_at='';await refresh()}catch(e){error.value=e?.response?.data?.error||'Не удалось создать назначения'}finally{saving.value=false}}
async function cancelAssignment(a){try{await api.patch(`/api/admin/questionnaire-assignments/${a.id}/cancel`);success.value='Назначение отменено';await refresh()}catch(e){error.value=e?.response?.data?.error||'Не удалось отменить назначение'}}
async function createAppointment(){savingAppointment.value=true;try{await api.post('/api/admin/appointments',{user_id:appointmentForm.user_id,scheduled_at:`${appointmentForm.date}T${appointmentForm.time}:00`,specialist:appointmentForm.specialist||null,doctor:appointmentForm.doctor||null,comment:appointmentForm.comment||null});success.value='Приём назначен';Object.assign(appointmentForm,{user_id:appointmentForm.user_id,date:'',time:'',specialist:'',doctor:'',comment:''})}catch(e){error.value=e?.response?.data?.error||'Не удалось назначить приём'}finally{savingAppointment.value=false}}
const statusLabel=v=>({pending:'Ожидает',in_progress:'В процессе',completed:'Завершён',cancelled:'Отменён'})[v]||v
const priorityLabel=v=>({low:'Низкий',normal:'Обычный',high:'Высокий',urgent:'Срочный'})[v]||v
const userLabel=u=>`${u.username} · ${u.age ?? '?'} лет · ${u.gender==='F'?'Ж':u.gender==='M'?'М':'?'}${u.email?` · ${u.email}`:''}`
const formatDate=v=>new Intl.DateTimeFormat('ru-RU',{day:'2-digit',month:'short',hour:'2-digit',minute:'2-digit'}).format(new Date(v))
</script>

<style scoped>
.page{min-height:100vh}.admin{width:min(1240px,calc(100% - 2rem));margin:auto;padding:2rem 0 4rem}.hero,.panel{border:1px solid #dce8ea;border-radius:10px;background:#fff;box-shadow:0 16px 40px rgba(25,45,65,.07)}.hero{display:flex;justify-content:space-between;gap:2rem;align-items:end;padding:2rem;margin-bottom:1rem}.eyebrow{margin:0 0 .6rem;color:#1f847f;font-weight:800;text-transform:uppercase;font-size:.8rem}h1,h2,p{margin-top:0}.hero p,header p{color:#637284}.stats{display:grid;grid-template-columns:repeat(4,100px);gap:.6rem}.stats div{padding:.8rem;border:1px solid #e2ecee;border-radius:8px;background:#f8fbfb}.stats strong,.stats span,.assignment small{display:block}.stats strong{font-size:1.4rem}.stats span,.assignment small{font-size:.78rem;color:#667788}.patient-panel,.antigen-panel,.incomplete-panel{margin-bottom:1rem}.patient-grid{display:grid;grid-template-columns:repeat(4,1fr);gap:.7rem}.antigen-create,.selector-grid{display:grid;grid-template-columns:1fr 1fr;gap:.7rem;align-items:end}.antigen-create{grid-template-columns:1fr 220px;margin-bottom:1rem}.antigen-create .field,.selector-grid .field,.compact{margin-bottom:0}.antigen-editor{display:grid;gap:.8rem;padding:1rem;border:1px solid #dce8ea;border-radius:8px;background:#fbfdfd}.editor-head{display:grid;grid-template-columns:1fr minmax(260px,.7fr);gap:1rem;align-items:end}.editor-head strong,.editor-head small{display:block}.editor-head small{margin-top:.25rem;color:#667788}.antigen-picker{display:grid;grid-template-columns:repeat(2,minmax(220px,1fr));max-height:360px;overflow:auto;gap:.4rem;margin:0;padding:.65rem;border:1px solid #dce8ea;border-radius:8px;background:#fff}.incomplete-row{display:grid;grid-template-columns:1fr 160px 180px 160px;gap:.7rem;align-items:end;padding:.9rem 0 0;border-top:1px solid #e2ecee}.workspace{display:grid;grid-template-columns:1.2fr .8fr;gap:1rem}.panel{padding:1.3rem}header{margin-bottom:1rem}header h2{margin-bottom:.25rem}.field{display:grid;gap:.4rem;margin-bottom:1rem}.field span,legend{font-size:.9rem;font-weight:700;color:#34475c}.field input,.field select,.field textarea,.toolbar input,.toolbar select{width:100%;min-height:42px;padding:0 .75rem;border:1px solid #d4e0e3;border-radius:8px;background:#fbfdfd}.field textarea{padding-top:.7rem}.row{display:grid;grid-template-columns:1fr 1fr;gap:.7rem}.picker{display:grid;grid-template-columns:1fr 1fr;gap:.5rem;margin:0 0 1rem;padding:.8rem;border:1px solid #dce8ea;border-radius:8px}.check{display:flex;gap:.6rem;padding:.65rem;border:1px solid #e1ecee;border-radius:8px}.check span,.check small{display:block}.check small{color:#718291;margin-top:.2rem;overflow-wrap:anywhere}.primary{width:100%;min-height:42px;border:0;border-radius:8px;background:#1f847f;color:#fff;font-weight:800}.primary:disabled{background:#9ab6b4}.message{margin:1rem 0;padding:1rem;border-radius:8px;background:#f8fbfb}.error{color:#9b241c;background:#fff3f2}.success{color:#1f6c45;background:#f0fbf4}.assignments{margin-top:1rem}.toolbar{display:grid;grid-template-columns:1fr 260px 180px;gap:.7rem}.list{display:grid;gap:.65rem}.assignment{display:grid;grid-template-columns:1fr 1fr auto;gap:1rem;align-items:center;padding:.9rem;border:1px solid #e1ecee;border-radius:8px;background:#fbfdfd}.assignment p{margin:.25rem 0 0;font-weight:700}.meta{display:grid;gap:.25rem;color:#697b8c;font-size:.86rem}.actions{display:grid;justify-items:end;gap:.5rem}.tag{width:max-content;padding:.3rem .55rem;border-radius:999px;font-size:.75rem;font-weight:800}.status-pending{background:#eef3f7}.status-in_progress{background:#fff6df;color:#946713}.status-completed{background:#e8f6ee;color:#1f6c45}.status-cancelled{background:#eee}.priority-urgent{background:#fee4e2;color:#b42318}.priority-high{background:#fff0d5;color:#9a5b00}.priority-normal,.priority-low{background:#edf7f6;color:#12635f}.cancel{min-height:36px;padding:0 .7rem;border:0;border-radius:8px;color:#a22}.empty{text-align:center;color:#718291}@media(max-width:900px){.hero{display:block}.stats{grid-template-columns:repeat(2,1fr);margin-top:1rem}.patient-grid,.antigen-create,.selector-grid,.editor-head,.incomplete-row,.workspace,.row,.toolbar,.assignment{grid-template-columns:1fr}.antigen-picker,.picker{grid-template-columns:1fr}.actions{justify-items:start}}
</style>
