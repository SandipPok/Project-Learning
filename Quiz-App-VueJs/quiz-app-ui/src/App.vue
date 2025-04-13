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
                <button
                  v-for="topic in topicsByCategory[category.name]"
                  :key="topic.id"
                  @click="handleTopicSelect(topic)"
                  :class="[
                    'w-full text-left px-6 py-2 text-sm rounded-md transition-colors',
                    selectedTopic?.id === topic.id
                      ? 'bg-indigo-50 text-indigo-700'
                      : 'text-gray-700 hover:bg-gray-50',
                  ]"
                >
                  {{ topic.title }}
                </button>
              </div>
            </div>
          </div>
        </div>

        <!-- Subtopics and Questions -->
        <div class="col-span-9 space-y-6">
          <div v-if="selectedTopic" class="bg-white rounded-lg shadow-sm">
            <div class="p-4 border-b">
              <div class="flex items-center gap-2">
                <span class="text-sm text-indigo-600">{{ selectedTopic.category }}</span>
                <ChevronRight class="w-4 h-4 text-gray-400" />
                <h2 class="text-lg font-semibold text-gray-900">{{ selectedTopic.title }}</h2>
              </div>
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
                    />
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
                <div class="flex items-center gap-2 text-sm text-gray-600">
                  <span
                    >Question {{ currentQuestionIndex + 1 }} of
                    {{ selectedSubtopicQuestions.length }}</span
                  >
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
                    answers[currentQuestion.id] === index
                      ? 'border-indigo-500 bg-indigo-50'
                      : 'border-gray-200 hover:border-indigo-200',
                  ]"
                >
                  <Circle
                    v-if="answers[currentQuestion.id] !== index"
                    class="w-5 h-5 text-gray-400"
                  />
                  <CheckCircle2
                    v-if="answers[currentQuestion.id] === index"
                    class="w-5 h-5 text-indigo-600"
                  />
                  <span>{{ option }}</span>
                </button>
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
                        : answers[question.id] !== undefined
                          ? 'bg-indigo-100 text-indigo-600'
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
} from 'lucide-vue-next'
import { ref, computed } from 'vue'

// Data - Decoupled structure
const topics = ref([
  { id: '1', title: 'C# Fundamentals', category: '.NET' },
  { id: '2', title: 'ASP.NET Core MVC', category: '.NET' },
  { id: '3', title: 'Entity Framework Core', category: '.NET' },
])

const subtopics = ref([
  { id: 'cs-basics', topicId: '1', title: 'Basic Concepts' },
  { id: 'cs-oop', topicId: '1', title: 'Object-Oriented Programming' },
  { id: 'mvc-basics', topicId: '2', title: 'MVC Pattern' },
  { id: 'ef-basics', topicId: '3', title: 'Basic Concepts' },
])

const questions = ref([
  {
    id: 'q1',
    subtopicId: 'cs-basics',
    text: 'What is the correct way to declare a constant in C#?',
    options: [
      'static readonly int MAX_VALUE = 100;',
      'const int MAX_VALUE = 100;',
      'final int MAX_VALUE = 100;',
      'immutable int MAX_VALUE = 100;',
    ],
    correctAnswer: 1,
    facts: [
      'Constants in C# are immutable values that are known at compile time.',
      'Constants are declared using the const keyword followed by the type and name.',
      'Constants must be initialized at the time of declaration.',
      'Only primitive types (such as int, string, bool) can be declared as constants.',
    ],
    examples: [
      'const int MAX_USERS = 100; // Integer constant',
      'const string CONNECTION_STRING = "Server=myServerAddress;Database=myDataBase;"; // String constant',
      'const double PI = 3.14159; // Double constant',
      "const char GRADE_A = 'A'; // Character constant",
    ],
  },
  {
    id: 'q2',
    subtopicId: 'cs-basics',
    text: 'Which of the following is a value type in C#?',
    options: ['string', 'object', 'int', 'dynamic'],
    correctAnswer: 2,
    facts: [
      'Value types in C# store the actual data directly in memory allocated on the stack.',
      'Reference types store a reference to the data, with the actual data stored on the heap.',
      'Value types include all numeric types, bool, char, and structs.',
      'Value types are copied when assigned to a new variable or passed to a method.',
    ],
    examples: [
      'int number = 42; // Value type',
      'bool isActive = true; // Value type',
      "char letter = 'A'; // Value type",
      'struct Point { public int X; public int Y; } // Custom value type',
    ],
  },
  {
    id: 'q3',
    subtopicId: 'cs-oop',
    text: 'What is the purpose of the "sealed" keyword in C#?',
    options: [
      'To prevent a class from being instantiated',
      'To prevent a class from being inherited',
      'To make a class thread-safe',
      'To make a class immutable',
    ],
    correctAnswer: 1,
    facts: [
      'The sealed keyword prevents other classes from inheriting from a class.',
      'Sealed classes can be instantiated and used normally.',
      'Methods and properties can also be marked as sealed in derived classes.',
      'Sealed classes can improve performance as the compiler can optimize certain aspects.',
    ],
    examples: [
      'public sealed class Logger { /* ... */ } // Cannot be inherited',
      'public class Shape { public virtual void Draw() { } }',
      'public class Circle : Shape { public sealed override void Draw() { } } // Method cannot be overridden in derived classes',
      'public class SpecialCircle : Circle { /* Cannot override Draw() */ }',
    ],
  },
  {
    id: 'q4',
    subtopicId: 'mvc-basics',
    text: 'Which of the following is responsible for handling user requests in ASP.NET Core MVC?',
    options: ['Model', 'View', 'Controller', 'Router'],
    correctAnswer: 2,
    facts: [
      'Controllers in MVC handle incoming HTTP requests and decide what response to send back.',
      'Controllers contain action methods that correspond to different routes and HTTP verbs.',
      'Controllers interact with models to retrieve or update data.',
      'Controllers select which view to render as the response.',
    ],
    examples: [
      'public class HomeController : Controller { public IActionResult Index() { return View(); } }',
      '[HttpPost] public IActionResult Create(ProductModel product) { /* Save product */ return RedirectToAction("Index"); }',
      '[Route("api/[controller]")] public class ProductsController : ControllerBase { /* API endpoints */ }',
      'public IActionResult Details(int id) { var product = _repository.GetById(id); return View(product); }',
    ],
  },
  {
    id: 'q5',
    subtopicId: 'mvc-basics',
    text: 'What is the purpose of the ViewBag in ASP.NET Core MVC?',
    options: [
      'To store session state',
      'To pass data from controller to view',
      'To handle form submissions',
      'To manage database connections',
    ],
    correctAnswer: 1,
    facts: [
      'ViewBag is a dynamic property that allows passing data from controller to view.',
      'ViewBag uses the dynamic feature of C# to create properties at runtime.',
      'ViewBag data is only available during the current request.',
      'ViewData is an alternative to ViewBag that uses a dictionary instead of dynamic properties.',
    ],
    examples: [
      'public IActionResult Index() { ViewBag.Title = "Home Page"; return View(); }',
      'In view: <h1>@ViewBag.Title</h1>',
      'ViewBag.UserList = new List<User>(); // Passing a collection',
      'ViewBag.CurrentDate = DateTime.Now; // Passing a DateTime object',
    ],
  },
  {
    id: 'q6',
    subtopicId: 'ef-basics',
    text: 'What is the purpose of DbContext in Entity Framework Core?',
    options: [
      'To handle HTTP requests',
      'To manage database connections and operations',
      'To render views',
      'To handle user authentication',
    ],
    correctAnswer: 1,
    facts: [
      'DbContext is the primary class that coordinates Entity Framework functionality for a data model.',
      'DbContext represents a session with the database, allowing querying and saving data.',
      'DbContext includes DbSet<T> properties that represent collections of entities in the database.',
      'DbContext manages change tracking, caching, and transaction management.',
    ],
    examples: [
      'public class ApplicationDbContext : DbContext { public DbSet<Customer> Customers { get; set; } }',
      'using (var context = new ApplicationDbContext()) { var customers = context.Customers.ToList(); }',
      'context.Customers.Add(new Customer { Name = "John Doe" }); context.SaveChanges();',
      'var customer = context.Customers.Find(1); customer.Name = "Jane Doe"; context.SaveChanges();',
    ],
  },
])

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

// Methods
const toggleCategory = category => {
  expandedCategories.value[category] = !expandedCategories.value[category]
}

const handleTopicSelect = topic => {
  selectedTopic.value = topic
  selectedSubtopic.value = null
  currentQuestionIndex.value = 0
}

const getSubtopicsForTopic = topicId => {
  return subtopics.value.filter(subtopic => subtopic.topicId === topicId)
}

const handleSubtopicSelect = subtopic => {
  selectedSubtopic.value = subtopic
  currentQuestionIndex.value = 0
  showFacts.value = false
  showExamples.value = false
}

const handleAnswerSelect = (questionId, answerIndex) => {
  answers.value[questionId] = answerIndex
}

const getProgress = subtopic => {
  const subtopicQuestions = questions.value.filter(q => q.subtopicId === subtopic.id)
  const answered = subtopicQuestions.filter(q => answers.value[q.id] !== undefined).length
  return Math.round((answered / subtopicQuestions.length) * 100) || 0
}

const handleNextQuestion = () => {
  if (currentQuestionIndex.value < selectedSubtopicQuestions.value.length - 1) {
    currentQuestionIndex.value++
    showFacts.value = false
    showExamples.value = false
  }
}

const handlePreviousQuestion = () => {
  if (currentQuestionIndex.value > 0) {
    currentQuestionIndex.value--
    showFacts.value = false
    showExamples.value = false
  }
}

const setCurrentQuestionIndex = index => {
  currentQuestionIndex.value = index
  showFacts.value = false
  showExamples.value = false
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

const handleFiles = files => {
  if (files.length > 0) {
    const file = files[0]
    const reader = new FileReader()
    reader.onload = e => {
      const content = e.target.result
      // Process the CSV content here
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
    handleFiles(e.dataTransfer.files)
  }
}
</script>
