// [資料層 1] 主圖 key / 詳細彈窗 key 對應的顯示名稱
const MUSCLE_LABELS = {
  // 正面
  frontTrapezius: '斜方肌',
  frontDeltoidLeft: '三角肌',
  frontDeltoidRight: '三角肌',
  chest: '胸大肌',
  bicepsLeft: '肱二頭肌',
  bicepsRight: '肱二頭肌',
  frontForearm: '前臂',
  abs: '腹直肌',
  sideabs: '腹斜肌',
  quads: '股四頭肌',
  shin: '小腿前側',
  // 背面
  backTrapezius: '斜方肌（背面）',
  rhomboid: '菱形肌',
  lats: '背闊肌',
  rearDelt: '後三角肌',
  rearDeltLeft: '後三角肌',
  rearDeltRight: '後三角肌',
  triceps: '肱三頭肌',
  backForearm: '前臂（背面）',
  glute: '臀大肌',
  hamstring: '大腿後側',
  calves: '小腿後側',
}


// [靜態詳細圖] 沒有互動細分時，彈窗直接載入的 SVG 檔
const DETAIL_SVGS = {
  chest: { src: 'svg/detail/chest.svg', height: '450px' },
  bicepsLeft: { src: 'svg/detail/biceps-detail-left.svg', height: '620px', width: 'auto', viewBox: '0 0 317 1149' },
  bicepsRight: { src: 'svg/detail/biceps-detail-right.svg', height: '620px', width: 'auto', viewBox: '0 0 315 1150' },
  // frontDeltoidLeft: { src: 'svg/detail/shoulder-left.svg', height: '400px' },
  // frontDeltoidRight: { src: 'svg/detail/shoulder-right.svg', height: '400px' },
  calves: { src: 'svg/detail/calvesBack.svg', height: '400px' }
}


// [主圖連動] 
const MUSCLE_LINK = {
  // 三角
  frontDeltoidLeft: ['frontDeltoidRight'],
  frontDeltoidRight: ['frontDeltoidLeft'],
  // 二頭
  bicepsLeft: ['bicepsRight'],
  bicepsRight: ['bicepsLeft']
}


// [器材] 
const EQUIPMENT_LABELS = {
  dumbbell: '啞鈴',
  barbell: '槓鈴',
  bodyweight: '徒手訓練',
  machine: '機械式'
}


// [主圖 hover 同側連動] 
const MUSCLE_HOVER_LINK = {
  // 三角
  frontDeltoidLeft: ['frontDeltoidRight'],
  frontDeltoidRight: ['frontDeltoidLeft'],
  // 二頭肌
  bicepsLeft: ['bicepsRight'],
  bicepsRight: ['bicepsLeft'],
  // 腓腸肌
  gastrocnemiusLeft: ['gastrocnemiusRight'],
  gastrocnemiusRight: ['gastrocnemiusLeft'],
  // 比目魚肌
  soleusLeft: ['soleusRight'],
  soleusRight: ['soleusLeft']
}


// [動作資料]
const EXERCISES = {
  frontTrapezius: [],
  frontDeltoidLeft: [],
  frontDeltoidRight: [],
  chest: [
    {
      name: '啞鈴臥推',
      equipment: 'dumbbell',
      difficulty: 2,
      muscles: [
        { name: '胸大肌', pct: 70 },
        { name: '肱三頭肌', pct: 20 },
        { name: '前三角肌', pct: 10 },
      ],
      desc: '1.平躺於臥推椅上，雙手各握一個啞鈴置於胸口兩側。\n2.肩胛微收並讓下背自然貼近椅面，雙腳穩定踩地。\n3.將啞鈴向上推至手臂接近伸直。\n4.緩慢下放回胸口兩側，保持手腕穩定。\n5.全程保持核心收緊，避免聳肩或借力。',
      img: '/images/exercises/dumbbell_bench_press.jpg'
    },
    {
      name: '槓鈴臥推',
      equipment: 'barbell',
      difficulty: 3,
      muscles: [
        { name: '胸大肌', pct: 65 },
        { name: '肱三頭肌', pct: 25 },
        { name: '前三角肌', pct: 10 },
      ],
      desc: '平躺於臥推椅上，雙手握槓距離<strong>略寬於肩</strong>。將槓鈴從架上取下，<strong>下放至胸口輕觸</strong>後推起至手臂伸直。保持<strong>肩胛收緊</strong>、<strong>腳踩地面</strong>穩定發力。',
      img: ''
    },
    {
      name: '伏地挺身',
      equipment: 'bodyweight',
      difficulty: 1,
      muscles: [
        { name: '胸大肌', pct: 60 },
        { name: '肱三頭肌', pct: 25 },
        { name: '前三角肌', pct: 15 },
      ],
      desc: '1.雙手放在地板上，略寬於肩。 \n 2.從頭到腳，將身體伸展成一條直線。 \n 3.保持肩膀位於手腕正上方（不要向前或向後）。 \n 4.收緊核心肌群，臀部內收，肋骨收緊。 \n 5.將重量均勻分佈在全身，而不僅僅是手腕上。',
      img: '/images/exercises/push-up.png'

    }
  ],
  bicepsLeft: [],
  bicepsRight: [],
  frontForearm: [],
  abs: [],
  sideabs: [],
  quads: [],
  shin: [],
  backTrapezius: [],
  rhomboid: [],
  lats: [],
  rearDelt: [],
  triceps: [],
  backForearm: [],
  glute: [],
  hamstring: [],
  calves: [],
}
// [前端備援動作] 資料庫沒有對應資料時，仍保證每個肌群 × 每種器材至少有一個動作可顯示與查看說明。
const FALLBACK_EQUIPMENT_ORDER = ['dumbbell', 'barbell', 'machine', 'bodyweight']

const MUSCLE_SUMMARY_GROUPS = {
  chest: 'chest',
  frontTrapezius: 'back',
  backTrapezius: 'back',
  frontDeltoidLeft: 'shoulder',
  frontDeltoidRight: 'shoulder',
  rearDelt: 'shoulder',
  rearDeltLeft: 'shoulder',
  rearDeltRight: 'shoulder',
  rhomboid: 'back',
  lats: 'back',
  bicepsLeft: 'arms',
  bicepsRight: 'arms',
  triceps: 'arms',
  frontForearm: 'arms',
  backForearm: 'arms',
  abs: 'core',
  sideabs: 'core',
  quads: 'legs',
  shin: 'legs',
  hamstring: 'legs',
  calves: 'legs',
  glute: 'glute'
}

const MUSCLE_EQUIPMENT_FALLBACKS = {
  frontTrapezius: { dumbbell: '啞鈴聳肩', barbell: '槓鈴聳肩', machine: '史密斯聳肩', bodyweight: '肩胛上提控制' },
  backTrapezius: { dumbbell: '啞鈴聳肩', barbell: '槓鈴聳肩', machine: '史密斯聳肩', bodyweight: '俯身肩胛後收' },
  frontDeltoidLeft: { dumbbell: '啞鈴肩推', barbell: '槓鈴肩推', machine: '肩推機', bodyweight: '派克伏地挺身' },
  frontDeltoidRight: { dumbbell: '啞鈴肩推', barbell: '槓鈴肩推', machine: '肩推機', bodyweight: '派克伏地挺身' },
  rearDelt: { dumbbell: '啞鈴反向飛鳥', barbell: '槓鈴後三角划船', machine: '反向飛鳥機', bodyweight: '俯臥 Y 字抬手' },
  rearDeltLeft: { dumbbell: '啞鈴反向飛鳥', barbell: '槓鈴後三角划船', machine: '反向飛鳥機', bodyweight: '俯臥 Y 字抬手' },
  rearDeltRight: { dumbbell: '啞鈴反向飛鳥', barbell: '槓鈴後三角划船', machine: '反向飛鳥機', bodyweight: '俯臥 Y 字抬手' },
  chest: { dumbbell: '啞鈴臥推', barbell: '槓鈴臥推', machine: '胸推機', bodyweight: '伏地挺身' },
  bicepsLeft: { dumbbell: '啞鈴彎舉', barbell: '槓鈴彎舉', machine: '滑輪彎舉', bodyweight: '反手澳式划船' },
  bicepsRight: { dumbbell: '啞鈴彎舉', barbell: '槓鈴彎舉', machine: '滑輪彎舉', bodyweight: '反手澳式划船' },
  triceps: { dumbbell: '啞鈴過頭伸展', barbell: '窄握槓鈴臥推', machine: '滑輪下壓', bodyweight: '鑽石伏地挺身' },
  frontForearm: { dumbbell: '啞鈴腕彎舉', barbell: '槓鈴腕彎舉', machine: '滑輪腕彎舉', bodyweight: '懸垂握力訓練' },
  backForearm: { dumbbell: '啞鈴反向腕彎舉', barbell: '槓鈴反向腕彎舉', machine: '滑輪反向腕彎舉', bodyweight: '毛巾懸垂' },
  abs: { dumbbell: '啞鈴負重捲腹', barbell: '槓鈴滾輪', machine: '腹部捲腹機', bodyweight: '棒式' },
  sideabs: { dumbbell: '啞鈴側彎', barbell: '槓鈴地雷管旋轉', machine: '滑輪伐木式旋轉', bodyweight: '側棒式' },
  quads: { dumbbell: '啞鈴高腳杯深蹲', barbell: '槓鈴深蹲', machine: '腿伸展機', bodyweight: '徒手深蹲' },
  shin: { dumbbell: '啞鈴脛前肌抬腳', barbell: '槓片脛前肌抬腳', machine: '脛前肌訓練機', bodyweight: '靠牆抬腳尖' },
  rhomboid: { dumbbell: '啞鈴俯身划船', barbell: '槓鈴划船', machine: '坐姿划船機', bodyweight: '肩胛伏地挺身' },
  lats: { dumbbell: '啞鈴上拉', barbell: '槓鈴划船', machine: '滑輪下拉', bodyweight: '引體向上' },
  glute: { dumbbell: '啞鈴臀推', barbell: '槓鈴臀推', machine: '臀部後踢機', bodyweight: '臀橋' },
  hamstring: { dumbbell: '啞鈴羅馬尼亞硬舉', barbell: '槓鈴羅馬尼亞硬舉', machine: '腿後勾機', bodyweight: '滑步腿後勾' },
  calves: { dumbbell: '啞鈴站姿提踵', barbell: '槓鈴站姿提踵', machine: '坐姿提踵機', bodyweight: '徒手提踵' }
}

function createFallbackExercise(muscleKey, equipment, name) {
  const muscleLabel = MUSCLE_LABELS[muscleKey] || muscleKey
  const cleanMuscleLabel = muscleLabel.replace(/（.*?）/g, '')
  const equipmentLabel = EQUIPMENT_LABELS[equipment] || equipment
  const summaryKey = MUSCLE_SUMMARY_GROUPS[muscleKey] || muscleKey
  const difficultyMap = { dumbbell: 2, barbell: 3, machine: 2, bodyweight: 1 }
  const repsMap = { dumbbell: '10-12', barbell: '8-10', machine: '10-15', bodyweight: '12-15' }

  return {
    name,
    equipment,
    equipmentLabel,
    muscle: muscleKey,
    muscleLabel: cleanMuscleLabel,
    difficulty: difficultyMap[equipment] || 2,
    muscles: [
      { key: summaryKey, name: cleanMuscleLabel, pct: 70 },
      { key: 'core', name: '核心穩定', pct: 15 }
    ],
    sets: 3,
    reps: repsMap[equipment] || '10-12',
    restSeconds: '45-60',
    desc: `1.使用${equipmentLabel}進行${name}，先調整姿勢並對準${cleanMuscleLabel}發力。\n2.動作過程保持核心收緊，避免聳肩、甩動或用其他部位代償。\n3.以可控制的速度完成向心與離心階段，感受${cleanMuscleLabel}穩定收縮。\n4.每組保留穩定動作品質，若姿勢跑掉就降低重量或減少次數。`,
    img: ''
  }
}

function ensureFallbackExercises() {
  Object.keys(MUSCLE_LABELS).forEach(muscleKey => {
    if (!EXERCISES[muscleKey]) EXERCISES[muscleKey] = []

    const fallbackSet = MUSCLE_EQUIPMENT_FALLBACKS[muscleKey]
    if (!fallbackSet) return

    FALLBACK_EQUIPMENT_ORDER.forEach(equipment => {
      const exists = EXERCISES[muscleKey].some(ex => ex.equipment === equipment && ex.name)
      if (exists) return

      EXERCISES[muscleKey].push(createFallbackExercise(muscleKey, equipment, fallbackSet[equipment]))
    })
  })
}

ensureFallbackExercises()
