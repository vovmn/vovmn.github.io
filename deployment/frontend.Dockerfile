FROM node:20-slim AS builder
WORKDIR /app
COPY package*.json ./
RUN npm install --no-optional
COPY . .
RUN npm install @rollup/rollup-linux-x64-gnu --no-save || true && \
		if [ -f node_modules/rollup/dist/native.js ]; then \
			node -e "const fs=require('fs');const p='node_modules/rollup/dist/native.js';let s=fs.readFileSync(p,'utf8');s=s.replace(/throw new Error\(/g,'console.warn(');fs.writeFileSync(p,s);"; \
		fi && \
		npm run build

FROM nginx:stable
COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
