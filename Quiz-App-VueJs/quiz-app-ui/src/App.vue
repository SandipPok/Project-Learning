<template>
  <div class="min-h-screen bg-gray-50">
    <header class="bg-white shadow-xs">
      <div class="max-w-7xl mx-auto px-4 py-4 sm:px-6 lg:px-8">
        <div class="flex justify-between items-center">
          <h1 class="text-2xl font-bold text-gray-900 flex items-center gap-2">
            <BookOpen class="w-6 h-6 text-indigo-600" />
            Interactive Quiz Platform
          </h1>
          <button
            @click="isImportModalOpen = true"
            class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-xs text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-hidden focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
          >
            <Upload class="w-4 h-4 mr-2" />
            Import Questions
          </button>
        </div>
      </div>
    </header>

    <main class="max-w-7xl mx-auto px-4 py-6 sm:px-6 lg:px-8">
      <div class="grid grid-cols-12 gap-6">
        <!-- Topics Sidebar -->
        <div class="col-span-3 bg-white rounded-lg shadow-sm">
          <div class="p-4 border-b">
            <div class="relative">
              <input
                type="text"
                placeholder="Search topics..."
                v-model="searchQuery"
                class="w-full px-4 py-2 pr-10 rounded-md border border-gray-300 focus:outline-hidden focus:ring-2 focus:ring-indigo-500 focus:border-transparent"
              />
              <Search
                class="w-5 h-5 text-gray-400 absolute right-3 top-1/2 transform -translate-y-1/2"
              />
            </div>
          </div>
          <div class="p-2">
            <div v-for="category in categories" :key="category.id" class="mb-2">
              <button
                @click="toggleCategory(category.name)"
                class="w-full flex items-center justify-between px-4 py-2 text-sm font-medium text-gray-900 hover:bg-gray-50"
              >
                {{ category.name }}
                <ChevronUp v-if="expandedCategories[category.name]" class="w-4 h-4" />
                <ChevronDown v-else class="w-4 h-4" />
              </button>
              <div v-if="expandedCategories[category.name]">
                <div
                  v-for="topic in topicsByCategory[category.name]"
                  :key="topic.id"
                  @click="handleTopicSelect(topic)"
                  :class="[
                    'px-4 py-3 rounded-lg transition-colors cursor-pointer',
                    'hover:bg-indigo-50 hover:border-indigo-100',
                    selectedTopic?.id === topic.id
                      ? 'bg-indigo-50 border border-indigo-200'
                      : 'border border-transparent',
                  ]"
                >
                  <div class="flex items-center justify-between">
                    <span class="font-medium text-gray-800">{{ topic.title }}</span>
                    <span class="text-xs text-indigo-600">{{ topic.progress }}%</span>
                  </div>

                  <!-- Progress Bar -->
                  <div class="mt-2 flex items-center gap-2">
                    <div class="w-full bg-gray-200 rounded-full h-2">
                      <div
                        class="bg-indigo-600 h-2 rounded-full transition-all duration-300"
                        :style="{ width: `${topic.progress}%` }"
                      ></div>
                    </div>
                    <span class="text-xs text-gray-500 whitespace-nowrap">
                      {{ topic.answered }}/{{ topic.total }} questions
                    </span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Subtopics and Questions -->
        <div class="col-span-9 space-y-6">
          <div v-if="selectedTopic" class="bg-white rounded-lg shadow-sm">
            <div class="p-4 border-b flex justify-between">
              <div class="flex items-center gap-2">
                <span class="text-sm text-indigo-600">{{ selectedTopic.category }}</span>
                <ChevronRight class="w-4 h-4 text-gray-400" />
                <h2 class="text-lg font-semibold text-gray-900">{{ selectedTopic.title }}</h2>
              </div>
              <button
                @click="resetSubtopicProgress"
                class="text-sm text-red-600 hover:text-red-800 flex items-center gap-1 cursor-pointer"
              >
                <RotateCcw class="w-4 h-4" />
                Reset
              </button>
            </div>
            <div class="p-4 grid grid-cols-2 gap-4">
              <div
                v-for="subtopic in getSubtopicsForTopic(selectedTopic.id)"
                :key="subtopic.id"
                @click="handleSubtopicSelect(subtopic)"
                :class="[
                  'p-4 rounded-lg border-2 transition-colors cursor-pointer',
                  selectedSubtopic?.id === subtopic.id
                    ? 'border-indigo-500 bg-indigo-50'
                    : 'border-gray-200 hover:border-indigo-200',
                ]"
              >
                <div class="flex justify-between items-center mb-2">
                  <h3 class="font-medium text-gray-900">{{ subtopic.title }}</h3>
                  <ChevronRight class="w-5 h-5 text-gray-400" />
                </div>
                <div class="flex items-center gap-2 text-sm text-gray-600">
                  <div class="w-full bg-gray-200 rounded-full h-2">
                    <div
                      class="bg-indigo-600 h-2 rounded-full"
                      :style="{ width: `${getProgress(subtopic)}%` }"
                    ></div>
                  </div>
                  <span>{{ getProgress(subtopic) }}%</span>
                </div>
              </div>
            </div>
          </div>

          <div v-if="currentQuestion" class="bg-white rounded-lg shadow-sm">
            <div class="p-4 border-b">
              <div class="flex items-center justify-between">
                <div class="flex items-center gap-2">
                  <span class="text-sm text-indigo-600">{{ selectedTopic?.title }}</span>
                  <ChevronRight class="w-4 h-4 text-gray-400" />
                  <span class="text-sm font-medium text-gray-900">{{
                    selectedSubtopic?.title
                  }}</span>
                </div>
                <div class="flex items-center gap-4">
                  <div class="flex items-center gap-2 text-sm text-gray-600">
                    <span
                      >Question {{ currentQuestionIndex + 1 }} of
                      {{ selectedSubtopicQuestions.length }}</span
                    >
                  </div>
                </div>
              </div>
            </div>
            <div class="p-6">
              <h3 class="text-xl font-medium text-gray-900 mb-4">{{ currentQuestion.text }}</h3>
              <div class="space-y-3">
                <button
                  v-for="(option, index) in currentQuestion.options"
                  :key="index"
                  @click="handleAnswerSelect(currentQuestion.id, index)"
                  :class="[
                    'w-full flex items-center gap-3 p-4 rounded-lg border-2 transition-colors',
                    currentQuestion.selectedAnswer === index
                      ? isAnswerCorrect(currentQuestion)
                        ? 'border-green-500 bg-green-50'
                        : 'border-red-500 bg-red-50'
                      : 'border-gray-200 hover:border-indigo-200',
                  ]"
                >
                  <template v-if="currentQuestion.selectedAnswer !== null">
                    <CheckCircle2
                      v-if="
                        isAnswerCorrect(currentQuestion) && currentQuestion.selectedAnswer === index
                      "
                      class="w-5 h-5 text-green-600"
                    />
                    <XCircle
                      v-else-if="
                        !isAnswerCorrect(currentQuestion) &&
                        currentQuestion.selectedAnswer === index
                      "
                      class="w-5 h-5 text-red-600"
                    />
                    <CheckCircle2
                      v-else-if="index === currentQuestion.correctAnswer"
                      class="w-5 h-5 text-green-600"
                    />
                    <Circle v-else class="w-5 h-5 text-gray-400" />
                  </template>
                  <Circle v-else class="w-5 h-5 text-gray-400" />
                  <span>{{ option }}</span>
                </button>
              </div>

              <!-- Score Section -->
              <div v-if="showScore" class="mt-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
                <h4 class="text-lg font-medium text-gray-800 flex items-center gap-2 mb-3">
                  <Award class="w-5 h-5" />
                  Your Progress
                </h4>
                <div class="grid grid-cols-3 gap-4">
                  <div class="text-center">
                    <div class="text-2xl font-bold text-indigo-600">{{ correctAnswersCount }}</div>
                    <div class="text-sm text-gray-600">Correct</div>
                  </div>
                  <div class="text-center">
                    <div class="text-2xl font-bold text-red-600">{{ incorrectAnswersCount }}</div>
                    <div class="text-sm text-gray-600">Incorrect</div>
                  </div>
                  <div class="text-center">
                    <div class="text-2xl font-bold text-gray-600">
                      {{
                        Math.round(
                          (correctAnswersCount / selectedSubtopicQuestions.length) * 100
                        ) || 0
                      }}%
                    </div>
                    <div class="text-sm text-gray-600">Score</div>
                  </div>
                </div>
              </div>

              <!-- Facts and Examples Buttons -->
              <div class="mt-6 flex gap-4">
                <button
                  @click="toggleFacts"
                  :class="[
                    'flex items-center gap-2 px-4 py-2 rounded-md border',
                    showFacts
                      ? 'bg-indigo-50 border-indigo-500 text-indigo-700'
                      : 'border-gray-300 text-gray-700 hover:bg-gray-50',
                  ]"
                >
                  <Info class="w-4 h-4" />
                  {{ showFacts ? 'Hide Facts' : 'Show Facts' }}
                </button>
                <button
                  @click="toggleExamples"
                  :class="[
                    'flex items-center gap-2 px-4 py-2 rounded-md border',
                    showExamples
                      ? 'bg-indigo-50 border-indigo-500 text-indigo-700'
                      : 'border-gray-300 text-gray-700 hover:bg-gray-50',
                  ]"
                >
                  <Code class="w-4 h-4" />
                  {{ showExamples ? 'Hide Examples' : 'Show Examples' }}
                </button>
              </div>

              <!-- Facts Section -->
              <div
                v-if="showFacts && currentQuestion.facts"
                class="mt-4 p-4 bg-blue-50 rounded-lg border border-blue-200"
              >
                <h4 class="text-lg font-medium text-blue-800 flex items-center gap-2 mb-3">
                  <Lightbulb class="w-5 h-5" />
                  Key Facts
                </h4>
                <ul class="space-y-2">
                  <li
                    v-for="(fact, index) in currentQuestion.facts"
                    :key="index"
                    class="flex items-start gap-2"
                  >
                    <span class="text-blue-500 font-bold mt-1">•</span>
                    <span class="text-blue-800">{{ fact }}</span>
                  </li>
                </ul>
              </div>

              <!-- Examples Section -->
              <div
                v-if="showExamples && currentQuestion.examples"
                class="mt-4 p-4 bg-gray-50 rounded-lg border border-gray-200"
              >
                <h4 class="text-lg font-medium text-gray-800 flex items-center gap-2 mb-3">
                  <Code class="w-5 h-5" />
                  Examples
                </h4>
                <div class="space-y-2">
                  <div
                    v-for="(example, index) in currentQuestion.examples"
                    :key="index"
                    class="bg-gray-800 text-gray-100 p-3 rounded-md font-mono text-sm overflow-x-auto"
                  >
                    {{ example }}
                  </div>
                </div>
              </div>
            </div>
            <div class="p-4 border-t bg-gray-50">
              <div class="flex justify-between items-center">
                <button
                  @click="handlePreviousQuestion"
                  :disabled="currentQuestionIndex === 0"
                  :class="[
                    'flex items-center gap-2 px-4 py-2 rounded-md',
                    currentQuestionIndex === 0
                      ? 'text-gray-400 cursor-not-allowed'
                      : 'text-indigo-600 hover:bg-indigo-50',
                  ]"
                >
                  <ArrowLeft class="w-4 h-4" />
                  Previous
                </button>
                <div class="flex gap-2">
                  <button
                    v-for="(question, index) in selectedSubtopicQuestions"
                    :key="index"
                    @click="setCurrentQuestionIndex(index)"
                    :class="[
                      'w-8 h-8 rounded-full flex items-center justify-center',
                      index === currentQuestionIndex
                        ? 'bg-indigo-600 text-white'
                        : question.selectedAnswer !== null
                          ? isAnswerCorrect(question)
                            ? 'bg-green-100 text-green-600'
                            : 'bg-red-100 text-red-600'
                          : 'bg-gray-200 text-gray-600',
                    ]"
                  >
                    {{ index + 1 }}
                  </button>
                </div>
                <button
                  @click="handleNextQuestion"
                  :disabled="currentQuestionIndex === selectedSubtopicQuestions.length - 1"
                  :class="[
                    'flex items-center gap-2 px-4 py-2 rounded-md',
                    currentQuestionIndex === selectedSubtopicQuestions.length - 1
                      ? 'text-gray-400 cursor-not-allowed'
                      : 'text-indigo-600 hover:bg-indigo-50',
                  ]"
                >
                  Next
                  <ArrowRight class="w-4 h-4" />
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </main>

    <!-- Import Modal -->
    <div
      v-if="isImportModalOpen"
      class="fixed inset-0 bg-black/50 flex items-center justify-center z-50"
    >
      <div class="bg-white rounded-lg shadow-xl max-w-2xl w-full mx-4">
        <div class="p-4 border-b flex justify-between items-center">
          <h2 class="text-lg font-semibold text-gray-900">Import Questions</h2>
          <button @click="isImportModalOpen = false" class="text-gray-400 hover:text-gray-500">
            <X class="w-5 h-5" />
          </button>
        </div>
        <div class="p-6">
          <div
            :class="[
              'border-2 border-dashed rounded-lg p-8 text-center',
              dragActive ? 'border-indigo-500 bg-indigo-50' : 'border-gray-300',
            ]"
            @dragenter="handleDrag"
            @dragleave="handleDrag"
            @dragover="handleDrag"
            @drop="handleDrop"
          >
            <input
              ref="fileInputRef"
              type="file"
              accept=".csv"
              @change="handleFiles"
              class="hidden"
            />
            <Upload class="w-12 h-12 text-gray-400 mx-auto mb-4" />
            <p class="text-lg font-medium text-gray-900 mb-1">Drag and drop your CSV file here</p>
            <p class="text-sm text-gray-500 mb-4">or click to browse from your computer</p>
            <button
              @click="fileInputRef.click()"
              class="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-indigo-700 bg-indigo-100 hover:bg-indigo-200 focus:outline-hidden focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
            >
              Browse Files
            </button>
          </div>
          <div class="mt-4">
            <h3 class="text-sm font-medium text-gray-900 mb-2">CSV Format Requirements:</h3>
            <ul class="text-sm text-gray-600 list-disc pl-5 space-y-1">
              <li>First column: Question text</li>
              <li>Second to fifth columns: Options (A, B, C, D)</li>
              <li>Sixth column: Correct answer (0-3)</li>
              <li>Seventh column: Topic</li>
              <li>Eighth column: Subtopic</li>
              <li>Ninth column: Facts (comma-separated)</li>
              <li>Tenth column: Examples (comma-separated)</li>
            </ul>
          </div>
        </div>
        <div class="p-4 bg-gray-50 border-t flex justify-end gap-3">
          <button
            @click="isImportModalOpen = false"
            class="px-4 py-2 text-sm font-medium text-gray-700 hover:bg-gray-100 rounded-md"
          >
            Cancel
          </button>
          <button
            @click="isImportModalOpen = false"
            class="px-4 py-2 text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 rounded-md"
          >
            Import
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import {
  BookOpen,
  ChevronRight,
  CheckCircle2,
  Circle,
  Search,
  ChevronDown,
  ChevronUp,
  ArrowLeft,
  ArrowRight,
  Upload,
  X,
  Info,
  Code,
  Lightbulb,
  RotateCcw,
  XCircle,
  Award,
} from 'lucide-vue-next'
import { ref, computed, onMounted } from 'vue'

// Data - Decoupled structure
const topics = ref([])

const subtopics = ref([])

const questions = ref([])

// Reactive state
const selectedTopic = ref(null)
const selectedSubtopic = ref(null)
const currentQuestionIndex = ref(0)
const answers = ref({})
const searchQuery = ref('')
const expandedCategories = ref({})
const showFacts = ref(false)
const showExamples = ref(false)
const isImportModalOpen = ref(false)
const dragActive = ref(false)
const fileInputRef = ref(null)
const showScore = ref(false)

// Computed properties
const categories = computed(() => {
  const uniqueCategories = [...new Set(topics.value.map(topic => topic.category))]
  return uniqueCategories.map(name => ({ id: name.toLowerCase(), name }))
})

const filteredTopics = computed(() => {
  if (!searchQuery.value) return topics.value
  return topics.value.filter(
    topic =>
      topic.title.toLowerCase().includes(searchQuery.value.toLowerCase()) ||
      topic.category.toLowerCase().includes(searchQuery.value.toLowerCase())
  )
})

const topicsByCategory = computed(() => {
  return categories.value.reduce((acc, category) => {
    acc[category.name] = filteredTopics.value.filter(topic => topic.category === category.name)
    return acc
  }, {})
})

const selectedSubtopicQuestions = computed(() => {
  if (!selectedSubtopic.value) return []
  return questions.value.filter(q => q.subtopicId === selectedSubtopic.value.id)
})

const currentQuestion = computed(() => {
  return selectedSubtopicQuestions.value[currentQuestionIndex.value] || null
})

const correctAnswersCount = computed(() => {
  if (!selectedSubtopic.value) return 0
  return selectedSubtopicQuestions.value.filter(
    q => q.selectedAnswer !== null && q.selectedAnswer === q.correctAnswer
  ).length
})

const incorrectAnswersCount = computed(() => {
  if (!selectedSubtopic.value) return 0
  return selectedSubtopicQuestions.value.filter(
    q => q.selectedAnswer !== null && q.selectedAnswer !== q.correctAnswer
  ).length
})

// Methods
const toggleCategory = category => {
  expandedCategories.value[category] = !expandedCategories.value[category]
}

const handleTopicSelect = async topic => {
  await fetch(`https://localhost:44358/api/v2/getSubTopics?id=${topic.id}`)
    .then(data => data.json())
    .then(res => {
      subtopics.value = res.subTopics
      questions.value = res.questions
    })
    .catch(err => console.error('Error fetching subtopics:', err))

  selectedTopic.value = topic
  selectedSubtopic.value = null
  currentQuestionIndex.value = 0
  showScore.value = false
}

const getSubtopicsForTopic = topicId => {
  return subtopics.value.filter(subtopic => subtopic.topicId === topicId)
}

const handleSubtopicSelect = async subtopic => {
  selectedSubtopic.value = subtopic
  currentQuestionIndex.value = 0
  showFacts.value = false
  showExamples.value = false
  showScore.value = false
}

const isAnswerCorrect = question => {
  return question.selectedAnswer === question.correctAnswer
}

const resetSubtopicProgress = () => {
  if (!selectedSubtopic.value) return

  // Reset selected answers for the current subtopic
  questions.value
    .filter(q => q.subtopicId === selectedSubtopic.value.id)
    .forEach(q => {
      q.selectedAnswer = null
    })

  // Update the progress for the current topic
  if (selectedTopic.value) {
    const topicSubtopics = getSubtopicsForTopic(selectedTopic.value.id)
    const topicQuestions = topicSubtopics.flatMap(subtopic =>
      questions.value.filter(q => q.subtopicId === subtopic.id)
    )

    const totalQuestions = topicQuestions.length
    const answeredQuestions = topicQuestions.filter(q => q.selectedAnswer !== null).length

    const progress = totalQuestions > 0 ? Math.round((answeredQuestions / totalQuestions) * 100) : 0

    // Update the topic's progress in the topics array
    const topicIndex = topics.value.findIndex(t => t.id === selectedTopic.value.id)
    if (topicIndex !== -1) {
      topics.value[topicIndex].progress = progress
      topics.value[topicIndex].answered = answeredQuestions
      topics.value[topicIndex].total = totalQuestions
    }
  }

  // Reset the current question index and hide the score
  currentQuestionIndex.value = 0
  showScore.value = false

  // Optionally, reset the progress on the backend
  fetch('https://localhost:44358/api/v2/updateQuestions', {
    method: 'PUT',
    headers: {
      'Content-Type': 'application/json',
    },
    body: JSON.stringify({
      selectedAnswer: null,
    }),
  }).catch(err => console.error('Error saving answer:', err))
}

const handleAnswerSelect = (questionId, answerIndex) => {
  // Prevent updating if the question is already answered
  if (questions.value.find(q => q.id === questionId).selectedAnswer !== null) return

  const question = questions.value.find(q => q.id === questionId)
  if (question) {
    // Update the selected answer locally
    question.selectedAnswer = answerIndex

    // Update the progress for the current topic
    if (selectedTopic.value) {
      const topicSubtopics = getSubtopicsForTopic(selectedTopic.value.id)
      const topicQuestions = topicSubtopics.flatMap(subtopic =>
        questions.value.filter(q => q.subtopicId === subtopic.id)
      )

      const totalQuestions = topicQuestions.length
      const answeredQuestions = topicQuestions.filter(q => q.selectedAnswer !== null).length

      const progress =
        totalQuestions > 0 ? Math.round((answeredQuestions / totalQuestions) * 100) : 0

      // Update the topic's progress in the topics array
      const topicIndex = topics.value.findIndex(t => t.id === selectedTopic.value.id)
      if (topicIndex !== -1) {
        topics.value[topicIndex].progress = progress
        topics.value[topicIndex].answered = answeredQuestions
        topics.value[topicIndex].total = totalQuestions
      }
    }

    // Optionally, save the selected answer to the backend
    fetch('https://localhost:44358/api/v2/updateQuestions', {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        id: questionId,
        selectedAnswer: answerIndex,
      }),
    }).catch(err => console.error('Error saving answer:', err))
  }

  showScore.value = true
}

const getProgress = subtopic => {
  const subtopicQuestions = questions.value.filter(q => q.subtopicId === subtopic.id)
  const answered = subtopicQuestions.filter(q => q.selectedAnswer !== null).length
  return Math.round((answered / subtopicQuestions.length) * 100) || 0
}

const handleNextQuestion = () => {
  if (currentQuestionIndex.value < selectedSubtopicQuestions.value.length - 1) {
    currentQuestionIndex.value++
    showFacts.value = false
    showExamples.value = false
    showScore.value = false
  }
}

const handlePreviousQuestion = () => {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
    showFacts.value = false
    showExamples.value = false
    showScore.value = false
  }
}

const setCurrentQuestionIndex = index => {
  currentQuestionIndex.value = index
  showFacts.value = false
  showExamples.value = false
  showScore.value = false
}

const toggleFacts = () => {
  showExamples.value = false
  showFacts.value = !showFacts.value
}

const toggleExamples = () => {
  showFacts.value = false
  showExamples.value = !showExamples.value
}

const setDragActive = value => {
  dragActive.value = value
}

const handleFiles = event => {
  const files = event.target.files || event.dataTransfer.files
  if (files.length > 0) {
    const file = files[0]
    const reader = new FileReader()
    reader.onload = e => {
      const content = e.target.result
      console.log('CSV content:', content)
    }
    reader.readAsText(file)
  }
}

const handleDrag = e => {
  e.preventDefault()
  e.stopPropagation()
  if (e.type === 'dragenter' || e.type === 'dragover') {
    setDragActive(true)
  } else if (e.type === 'dragleave') {
    setDragActive(false)
  }
}

const handleDrop = e => {
  e.preventDefault()
  e.stopPropagation()
  setDragActive(false)

  if (e.dataTransfer.files && e.dataTransfer.files[0]) {
    handleFiles(e)
  }
}

const fetchTopicProgress = async topicId => {
  try {
    const response = await fetch(
      `https://localhost:44358/api/v2/getTopicProgress?topicId=${topicId}`
    )
    const data = await response.json()
    return data.progress || 0
  } catch (error) {
    console.error(`Error fetching progress for topic ${topicId}:`, error)
    return 0
  }
}

// // Load answers when component mounts
onMounted(() => {
  loadAllTopics()
})

async function loadAllTopics() {
  try {
    const response = await fetch('https://localhost:44358/api/v2/getTopics')
    const topicsData = await response.json()

    // Fetch progress for each topic
    const topicsWithProgress = await Promise.all(
      topicsData.map(async topic => {
        const progress = await fetchTopicProgress(topic.id)
        return { ...topic, progress }
      })
    )

    topics.value = topicsWithProgress
  } catch (error) {
    console.error('Error fetching topics:', error)
  }
}
</script>
