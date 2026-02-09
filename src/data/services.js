// src/data/services.js

export const serviceSections = [
  {
    id: 'countertops',
    title: 'Столешницы',
    items: [
      { id: 'euro-cut-countertop', name: 'Еврозапил столешницы', description: 'Точный еврозапил для стыковки столешницы.', price: '1 900 руб/шт' },
      { id: 'edging-countertop-plastic', name: 'Кромление столешницы (пластик)', description: 'Кромление столешницы пластиком по торцам.', price: '250 руб/м' },
      { id: 'edging-countertop-nonstd-pvc', name: 'Кромление столешницы и нестандартных толщин (ПВХ)', description: 'Кромление столешниц и нестандартных толщин ПВХ-кромкой.', price: '85 руб/м' },
      { id: 'cut-countertop-26-3', name: 'Раскрой столешницы 26 мм (3)', description: 'Раскрой столешницы толщиной 26 мм.', price: '300 руб/шт' },
      { id: 'cut-countertop-38-3', name: 'Раскрой столешницы 38 мм (3)', description: 'Раскрой столешницы толщиной 38 мм.', price: '350 руб/шт' },
      { id: 'cut-countertop-38-42', name: 'Раскрой столешницы 38 мм (4,2)', description: 'Раскрой столешницы 38 мм (позиция 4,2).', price: '400 руб/шт' },
      { id: 'saw-countertop', name: 'Рез столешницы', description: 'Рез/распил столешницы.', price: '300 руб/шт' }
    ]
      },
    
  {
    id: 'edging',
    title: 'Кромление',
    items: [
      { id: 'edging-curved-2pvc', name: 'Кромление криволинейной 2×ПВХ', description: 'Кромление криволинейных деталей двойной ПВХ-кромкой.', price: '80 руб/м' },
      { id: 'edging-pvc-04-2', name: 'Кромление ПВХ 0,4–2 мм', description: 'Кромление ПВХ-кромкой толщиной 0,4–2 мм.', price: '40 руб/м' },
      { id: 'edging-pvc-2', name: 'Кромление ПВХ 2 мм', description: 'Кромление ПВХ-кромкой 2 мм (усиленный торец).', price: '50 руб/м' }
    ]
  },

  {
    id: 'cutting',
    title: 'Раскрой',
    items: [
      { id: 'cutting-map', name: 'Карта раскроя (1)', description: 'Подготовка схемы раскроя для оптимального расхода материала.', price: '100 руб/л' },

      { id: 'cut-ldvp', name: 'Раскрой ЛДВП', description: 'Раскрой листов ЛДВП по размерам.', price: '250 руб/лист' },
      { id: 'cut-hdf', name: 'Раскрой ХДФ', description: 'Раскрой листов ХДФ по размерам.', price: '250 руб/л' },

      { id: 'cut-ldsp-5', name: 'Раскрой ЛДСП (5)', description: 'Раскрой ЛДСП (позиция 5 по прайсу).', price: '500 руб/лист' },
      { id: 'cut-ldsp-6', name: 'Раскрой ЛДСП (6)', description: 'Раскрой ЛДСП (позиция 6 по прайсу).', price: '550 руб/лист' },
      { id: 'cut-ldsp-client-sheets', name: 'Раскрой ЛДСП (листы клиента)', description: 'Раскрой ЛДСП из материала клиента.', price: '600 руб/л' },
      { id: 'cut-ldsp-26', name: 'Раскрой ЛДСП 26 мм', description: 'Раскрой ЛДСП толщиной 26 мм.', price: '600 руб/лист' },

      { id: 'cut-furniture-board-3', name: 'Раскрой мебельного щита (3)', description: 'Раскрой мебельного щита.', price: '250 руб/шт' },
      { id: 'cut-ldsp-remnants', name: 'Раскрой остатков ЛДСП', description: 'Раскрой остатков ЛДСП (малые элементы).', price: '350 руб/шт' }
    ]
  },

  {
    id: 'drilling',
    title: 'Сверление и пазы',
    items: [
      { id: 'hole-euroscrew', name: 'Отверстие под евровинт', description: 'Сверление под евровинт (конфирмат).', price: '10 руб/шт' },
      { id: 'hole-hinge', name: 'Отверстие под петлю', description: 'Сверление под мебельную петлю (чашка).', price: '40 руб/шт' },
      { id: 'holes-ties-no-eurocut', name: 'Отверстия под стяжки (без еврозапила)', description: 'Сверление отверстий под стяжки.', price: '600 руб/шт' },
      { id: 'groove-ldvp', name: 'Паз под ЛДВП', description: 'Выборка паза под заднюю стенку (ЛДВП).', price: '50 руб/м' }
    ]
  },

  {
    id: 'sawing',
    title: 'Рез',
    items: [
      { id: 'bevel-cut', name: 'Косой рез', description: 'Рез под углом (косой рез) по заданию.', price: '150 руб/шт' },
      { id: 'saw-ldsp', name: 'Рез ЛДСП', description: 'Рез/распил ЛДСП.', price: '120 руб/шт' }
    ]
  },

  {
    id: 'custom',
    title: 'Нестандарт и индивидуальные работы',
    items: [
      { id: 'nonstandard-thickness-glue', name: 'Изготовление нестандартных толщин (склейка)', description: 'Склейка для получения нестандартной толщины детали.', price: '450 руб/м²' },
      { id: 'oval-making', name: 'Изготовление овала', description: 'Фигурная обработка: овал по заданному размеру.', price: '600 руб/шт' },
      { id: 'radius-making', name: 'Изготовление радиуса', description: 'Скругление (радиус) по шаблону/размеру.', price: '400 руб/шт' },
      { id: 'custom-order-ready', name: 'Индивидуальный заказ (готовое изделие)', description: 'Изготовление изделия по индивидуальному ТЗ.', price: '1 550 руб/шт' }
    ]
  }
]
