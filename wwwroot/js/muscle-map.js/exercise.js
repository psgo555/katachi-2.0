const params = new URLSearchParams(window.location.search)
const exerciseKey = params.get('key')
const muscleName = params.get('muscle')
const equipmentName = params.get('equipment')
const exerciseName = params.get('name')

const muscleNameEl = document.querySelector('#ex-muscle-name')
const titleEl = document.querySelector('#ex-title')
const badgeEl = document.querySelector('#ex-badge')
const imgEl = document.querySelector('#ex-img')
const descEl = document.querySelector('#ex-desc')
const prescriptionSectionEl = document.querySelector('#ex-prescription-section')
const prescriptionEl = document.querySelector('#ex-prescription')

function hideMedia() {
  if (!imgEl || !imgEl.parentElement) return
  imgEl.removeAttribute('src')
  imgEl.alt = ''
  imgEl.style.display = 'none'
}

function normalizeText(value) {
  return String(value || '').replace(/\s+/g, '').trim()
}

function renderDescription(value) {
  const raw = String(value || '暫無說明').trim()
  const normalized = raw.replace(/\r\n/g, '\n').replace(/\n\s+/g, '\n')

  if (/^\s*\d+[.、]/m.test(normalized)) {
    descEl.innerHTML = normalized.replace(/\n/g, '<br>')
    return
  }

  const steps = normalized
    .replace(/。/g, '。\n')
    .split('\n')
    .map(step => step.trim())
    .filter(Boolean)

  descEl.innerHTML = steps.length > 1
    ? steps.map((step, index) => `${index + 1}.${step}`).join('<br>')
    : normalized
}

function formatRestSeconds(value) {
  const text = String(value || '').trim()
  if (!text) return ''
  return text.includes('秒') ? text : `${text} 秒`
}

function renderPrescription(exercise) {
  if (!prescriptionSectionEl || !prescriptionEl) return

  const sets = exercise.sets || exercise.prescription?.sets
  const reps = exercise.reps || exercise.prescription?.reps
  const rest = formatRestSeconds(exercise.restSeconds || exercise.rest || exercise.prescription?.rest)

  if (!sets && !reps && !rest) {
    prescriptionSectionEl.style.display = 'none'
    return
  }

  const setsText = sets ? `${sets} 組` : '建議組數'
  const repsText = reps ? `${reps} 次` : '建議次數'
  const restText = rest || '依狀態調整'

  prescriptionEl.textContent = `${setsText} × ${repsText}　休息時間 ${restText}`
  prescriptionSectionEl.style.display = ''
}

function findStaticExercise() {
  if (typeof EXERCISES === 'undefined' || !EXERCISES) return null

  if (muscleName && EXERCISES[muscleName]) {
    const direct = EXERCISES[muscleName].find(ex => normalizeText(ex.name) === normalizeText(exerciseName))
    if (direct) return { ...direct, muscle: muscleName }
  }

  const targetName = normalizeText(exerciseName)
  for (const [key, list] of Object.entries(EXERCISES)) {
    const matched = (list || []).find(ex => normalizeText(ex.name) === targetName)
    if (matched) return { ...matched, muscle: key }
  }

  return null
}

async function fetchDbExercise() {
  if (!exerciseKey && !exerciseName) return null

  const query = new URLSearchParams()
  if (exerciseKey) query.set('key', exerciseKey)
  if (exerciseName) query.set('name', exerciseName)

  try {
    const response = await fetch(`/MuscleMap/ExerciseData?${query.toString()}`)
    if (!response.ok) return null
    return await response.json()
  } catch (error) {
    console.warn('Exercise database data unavailable.', error)
    return null
  }
}

function renderMedia(exercise) {
  if (!exercise.img) {
    hideMedia()
    return
  }

  imgEl.style.display = ''
  imgEl.onerror = () => hideMedia()

  if (exercise.img.toLowerCase().endsWith('.mp4')) {
    imgEl.outerHTML = `
      <video class="ex-img" controls autoplay muted loop playsinline>
        <source src="${exercise.img}" type="video/mp4">
      </video>
    `
    return
  }

  imgEl.src = exercise.img
  imgEl.alt = exercise.name || ''
}

function renderDifficulty(exercise) {
  const difficulty = Number(exercise.difficulty) || 1

  const starsEl = document.querySelector('#ex-stars')
  starsEl.innerHTML = ''

  for (let i = 1; i <= 5; i++) {
    const star = document.createElement('span')
    star.className = 'ex-star' + (i <= difficulty ? ' ex-star--filled' : '')
    star.textContent = '\u2605'
    starsEl.appendChild(star)
  }

  document.querySelector('#ex-difficulty-wrap').style.display = ''
  return true
}

function renderMuscles(exercise) {
  const hasMuscles = Array.isArray(exercise.muscles) && exercise.muscles.length > 0
  if (!hasMuscles) return false

  const barsEl = document.querySelector('#ex-muscle-bars')
  barsEl.innerHTML = ''

  exercise.muscles.forEach(m => {
    let barColor
    if (m.pct >= 50) {
      barColor = 'linear-gradient(90deg, #ff9146, #d4845a)'
    } else if (m.pct >= 20) {
      barColor = 'linear-gradient(90deg, #ffd76a, #ff9146)'
    } else {
      barColor = 'linear-gradient(90deg, #d4d0ca, #ffd76a)'
    }

    const row = document.createElement('div')
    row.className = 'ex-muscle-row'
    row.innerHTML = `
      <span class="ex-muscle-label">${m.name}</span>
      <div class="ex-muscle-bar-bg">
        <div class="ex-muscle-bar-fill" style="width:${m.pct}%; background:${barColor}"></div>
      </div>
      <span class="ex-muscle-pct">${m.pct}%</span>
    `
    barsEl.appendChild(row)
  })

  document.querySelector('#ex-muscles-wrap').style.display = ''
  return true
}

function renderExercise(exercise) {
  if (!muscleNameEl || !titleEl || !badgeEl || !imgEl || !descEl || !exercise) {
    hideMedia()
    if (titleEl) titleEl.textContent = exerciseName || ''
    if (badgeEl) badgeEl.textContent = EQUIPMENT_LABELS[equipmentName] || ''
    if (muscleNameEl) muscleNameEl.textContent = MUSCLE_LABELS[muscleName] || ''
    if (descEl) descEl.textContent = '\u627e\u4e0d\u5230\u52d5\u4f5c\u8cc7\u6599'
    if (prescriptionSectionEl) prescriptionSectionEl.style.display = 'none'
    return
  }

  const resolvedMuscle = exercise.muscle || muscleName
  const resolvedEquipment = exercise.equipment || equipmentName

  muscleNameEl.textContent = exercise.muscleLabel || MUSCLE_LABELS[resolvedMuscle] || ''
  titleEl.textContent = exercise.name || exerciseName || ''
  badgeEl.textContent = exercise.equipmentLabel || EQUIPMENT_LABELS[resolvedEquipment] || resolvedEquipment || ''
  renderDescription(exercise.desc || '\u66ab\u7121\u8aaa\u660e')
  renderPrescription(exercise)

  renderMedia(exercise)

  const hasDifficulty = renderDifficulty(exercise)
  const hasMuscles = renderMuscles(exercise)

  if (hasDifficulty || hasMuscles) {
    document.querySelector('#ex-stats-card').style.display = ''
  }

  if (hasDifficulty && hasMuscles) {
    document.querySelector('#ex-stats-divider').style.display = ''
  }
}

async function initExercisePage() {
  const staticExercise = findStaticExercise()
  const dbExercise = await fetchDbExercise()

  const exercise = dbExercise
    ? {
        ...staticExercise,
        ...dbExercise,
        desc: staticExercise?.desc || dbExercise.desc,
        img: dbExercise.img || staticExercise?.img || ''
      }
    : staticExercise

  renderExercise(exercise)
}

initExercisePage()
