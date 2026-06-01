using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using PhysX.Models;

namespace PhysX;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly Stack<NavigationSnapshot> _backStack = new();
    private AppScreen _currentScreen = AppScreen.Home;
    private Topic _selectedTopic;
    private string _cardTaskDescription = "Здесь четыре направления карточек. Практический экзамен пока закрыт, потому что задачи ты добавишь позже.";
    private ObservableCollection<LearningOption> _cardTaskGroups = new();
    private string _cardTaskTitle = "Задания с карточками";
    private string _detailDescription = string.Empty;
    private ObservableCollection<LearningOption> _detailCards = new();
    private string _detailTitle = string.Empty;
    private IReadOnlyList<LessonPage> _lessonPages = Array.Empty<LessonPage>();
    private int _lessonPageIndex;

    public MainWindow()
    {
        Topics = new ObservableCollection<Topic>(CreateTopics());
        ElectricitySections = new ObservableCollection<LearningOption>(CreateElectricitySections());
        ElectrostaticsSections = new ObservableCollection<LearningOption>(CreateElectrostaticsSections());
        CardTaskGroups = new ObservableCollection<LearningOption>(CreateElectrostaticsCardTaskGroups());
        _selectedTopic = Topics[0];

        InitializeComponent();
        DataContext = this;
    }

    public ObservableCollection<Topic> Topics { get; }

    public ObservableCollection<LearningOption> ElectricitySections { get; }

    public ObservableCollection<LearningOption> ElectrostaticsSections { get; }

    public ObservableCollection<LearningOption> CardTaskGroups
    {
        get => _cardTaskGroups;
        private set
        {
            _cardTaskGroups = value;
            OnPropertyChanged();
        }
    }

    public string CardTaskTitle
    {
        get => _cardTaskTitle;
        private set
        {
            _cardTaskTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HeaderTitle));
        }
    }

    public string CardTaskDescription
    {
        get => _cardTaskDescription;
        private set
        {
            _cardTaskDescription = value;
            OnPropertyChanged();
        }
    }

    public ObservableCollection<LearningOption> DetailCards
    {
        get => _detailCards;
        private set
        {
            _detailCards = value;
            OnPropertyChanged();
        }
    }

    public Topic SelectedTopic
    {
        get => _selectedTopic;
        set
        {
            if (_selectedTopic == value)
            {
                return;
            }

            _selectedTopic = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(StartButtonText));
        }
    }

    public string DetailTitle
    {
        get => _detailTitle;
        private set
        {
            _detailTitle = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HeaderTitle));
        }
    }

    public string DetailDescription
    {
        get => _detailDescription;
        private set
        {
            _detailDescription = value;
            OnPropertyChanged();
        }
    }

    public string HeaderTitle => _currentScreen switch
    {
        AppScreen.Home => "Физика в симуляциях",
        AppScreen.ElectricityMenu => "Электричество",
        AppScreen.ElectrostaticsMenu => "Электростатика",
        AppScreen.CardTasksMenu => CardTaskTitle,
        AppScreen.DetailPage => DetailTitle,
        AppScreen.ChargeLesson => DetailTitle,
        _ => "PhysX",
    };

    public string CardTaskHeaderTitle => CardTaskTitle;

    public string StartButtonText => $"Открыть: {SelectedTopic.Title}";

    public bool CanGoBack => _backStack.Count > 0;

    public Visibility HomeVisibility => _currentScreen == AppScreen.Home
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility ElectricityMenuVisibility => _currentScreen == AppScreen.ElectricityMenu
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility ElectrostaticsMenuVisibility => _currentScreen == AppScreen.ElectrostaticsMenu
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility CardTasksMenuVisibility => _currentScreen == AppScreen.CardTasksMenu
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility DetailPageVisibility => _currentScreen == AppScreen.DetailPage
        ? Visibility.Visible
        : Visibility.Collapsed;

    public Visibility LessonPageVisibility => _currentScreen == AppScreen.ChargeLesson
        ? Visibility.Visible
        : Visibility.Collapsed;

    public LessonPage? CurrentLessonPage => _lessonPages.Count == 0
        ? null
        : _lessonPages[_lessonPageIndex];

    public string LessonProgressText => _lessonPages.Count == 0
        ? "Страница 0 из 0"
        : $"Страница {_lessonPageIndex + 1} из {_lessonPages.Count}";

    public bool CanGoPreviousLessonPage => _lessonPageIndex > 0;

    public bool CanGoNextLessonPage => _lessonPageIndex < _lessonPages.Count - 1;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OpenSelectedTopic(object sender, RoutedEventArgs e)
    {
        if (SelectedTopic.Id == "electrostatics")
        {
            NavigateTo(AppScreen.ElectrostaticsMenu);
            return;
        }

        if (SelectedTopic.Id == "electricity")
        {
            NavigateTo(AppScreen.ElectricityMenu);
            return;
        }

        MessageBox.Show(
            $"Раздел \"{SelectedTopic.Title}\" пока оставлен как каркас. Сейчас наполняем электростатику.",
            "PhysX",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void OpenElectrostaticsSection(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LearningOption option })
        {
            return;
        }

        switch (option.Id)
        {
            case "theory":
                ShowDetailPage(
                    option.Title,
                    "Теория электростатики: заряд, поле, взаимодействие зарядов и поляризация.",
                    CreateElectrostaticsTheoryCards());
                break;
            case "cards":
                SetCardTasks(
                    "Задания с карточками",
                    "Карточки по электростатике: знаки зарядов, обозначения, поле и сила. Практический экзамен пока закрыт.",
                    CreateElectrostaticsCardTaskGroups());
                NavigateTo(AppScreen.CardTasksMenu);
                break;
            case "lab":
                ShowDetailPage(
                    option.Title,
                    "Место для будущих опытов с зарядами, линиями поля и пробными частицами.",
                    CreateLabCards());
                break;
            case "reference":
                ShowDetailPage(
                    option.Title,
                    "Формулы, обозначения и короткие подсказки по электростатике.",
                    CreateReferenceCards());
                break;
        }
    }

    private void OpenElectricitySection(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LearningOption option })
        {
            return;
        }

        switch (option.Id)
        {
            case "electricity-theory":
                ShowDetailPage(
                    option.Title,
                    "Теория электричества: ток, напряжение, сопротивление и первые законы цепей.",
                    CreateElectricityTheoryCards());
                break;
            case "electricity-cards":
                SetCardTasks(
                    "Задания с карточками",
                    "Карточки по электрическим цепям: элементы, обозначения, свойства соединений и подготовка к практическому экзамену.",
                    CreateElectricityCardTaskGroups());
                NavigateTo(AppScreen.CardTasksMenu);
                break;
            case "electricity-lab":
                ShowDetailPage(
                    option.Title,
                    "Будущий конструктор простых цепей: источник, ключ, лампа, резистор и измерительные приборы.",
                    CreateElectricityLabCards());
                break;
            case "electricity-reference":
                ShowDetailPage(
                    option.Title,
                    "Справочник по формулам, обозначениям и единицам для первых задач по электрическим цепям.",
                    CreateElectricityReferenceCards());
                break;
        }
    }

    private void OpenCardTaskGroup(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LearningOption option })
        {
            return;
        }

        ShowDetailPage(
            option.Title,
            option.Description,
            CreateCardTaskDetailCards(option.Id));
    }

    private void OpenDetailCard(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: LearningOption option })
        {
            return;
        }

        if (option.Id == "charge")
        {
            ShowChargeLesson();
            return;
        }

        if (option.Id == "current")
        {
            ShowElectricCurrentLesson();
            return;
        }

        MessageBox.Show(
            $"Материал \"{option.Title}\" пока оставлен как заготовка. Наполним его на следующем проходе.",
            "PhysX",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }

    private void PreviousLessonPage(object sender, RoutedEventArgs e)
    {
        if (!CanGoPreviousLessonPage)
        {
            return;
        }

        _lessonPageIndex--;
        NotifyLessonPageChanged();
    }

    private void NextLessonPage(object sender, RoutedEventArgs e)
    {
        if (!CanGoNextLessonPage)
        {
            return;
        }

        _lessonPageIndex++;
        NotifyLessonPageChanged();
    }

    private void GoBack(object sender, RoutedEventArgs e)
    {
        if (_backStack.Count == 0)
        {
            return;
        }

        var snapshot = _backStack.Pop();
        _currentScreen = snapshot.Screen;
        DetailTitle = snapshot.DetailTitle;
        DetailDescription = snapshot.DetailDescription;
        DetailCards = new ObservableCollection<LearningOption>(snapshot.DetailCards);
        _lessonPages = snapshot.LessonPages;
        _lessonPageIndex = snapshot.LessonPageIndex;
        NotifyNavigationChanged();
        NotifyLessonPageChanged();
    }

    private void OpenSettings(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new SettingsWindow
        {
            Owner = this,
        };

        settingsWindow.ShowDialog();
    }

    private void OpenImagePreview(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: string imageSource })
        {
            return;
        }

        var previewWindow = new ImagePreviewWindow(imageSource, "Просмотр изображения")
        {
            Owner = this,
        };

        previewWindow.ShowDialog();
    }

    private void NavigateTo(AppScreen screen)
    {
        if (_currentScreen == screen)
        {
            return;
        }

        _backStack.Push(CaptureNavigation());
        _currentScreen = screen;
        NotifyNavigationChanged();
    }

    private void ShowDetailPage(
        string title,
        string description,
        IEnumerable<LearningOption> cards)
    {
        _backStack.Push(CaptureNavigation());
        DetailTitle = title;
        DetailDescription = description;
        DetailCards = new ObservableCollection<LearningOption>(cards);
        _currentScreen = AppScreen.DetailPage;
        NotifyNavigationChanged();
    }

    private void ShowChargeLesson()
    {
        _backStack.Push(CaptureNavigation());
        DetailTitle = "Электрический заряд";
        _lessonPages = CreateChargeLessonPages().ToArray();
        _lessonPageIndex = 0;
        _currentScreen = AppScreen.ChargeLesson;
        NotifyNavigationChanged();
        NotifyLessonPageChanged();
    }

    private void ShowElectricCurrentLesson()
    {
        _backStack.Push(CaptureNavigation());
        DetailTitle = "Электрический ток";
        _lessonPages = CreateElectricCurrentLessonPages().ToArray();
        _lessonPageIndex = 0;
        _currentScreen = AppScreen.ChargeLesson;
        NotifyNavigationChanged();
        NotifyLessonPageChanged();
    }

    private void SetCardTasks(
        string title,
        string description,
        IEnumerable<LearningOption> cards)
    {
        CardTaskTitle = title;
        CardTaskDescription = description;
        CardTaskGroups = new ObservableCollection<LearningOption>(cards);
    }

    private NavigationSnapshot CaptureNavigation()
    {
        return new NavigationSnapshot(
            _currentScreen,
            DetailTitle,
            DetailDescription,
            DetailCards.ToArray(),
            _lessonPages.ToArray(),
            _lessonPageIndex);
    }

    private void NotifyNavigationChanged()
    {
        OnPropertyChanged(nameof(HeaderTitle));
        OnPropertyChanged(nameof(CanGoBack));
        OnPropertyChanged(nameof(HomeVisibility));
        OnPropertyChanged(nameof(ElectricityMenuVisibility));
        OnPropertyChanged(nameof(ElectrostaticsMenuVisibility));
        OnPropertyChanged(nameof(CardTasksMenuVisibility));
        OnPropertyChanged(nameof(DetailPageVisibility));
        OnPropertyChanged(nameof(LessonPageVisibility));
    }

    private void NotifyLessonPageChanged()
    {
        OnPropertyChanged(nameof(CurrentLessonPage));
        OnPropertyChanged(nameof(LessonProgressText));
        OnPropertyChanged(nameof(CanGoPreviousLessonPage));
        OnPropertyChanged(nameof(CanGoNextLessonPage));
    }

    private static IEnumerable<Topic> CreateTopics()
    {
        return new[]
        {
            CreateTopic(
                "electricity",
                "Электричество",
                "Будущий раздел",
                "Ток, напряжение, сопротивление и первые электрические цепи.",
                "КАРКАС",
                new[] { "Теория", "Ток", "Цепи" },
                5,
                "#1D63C9"),
            CreateTopic(
                "electrostatics",
                "Электростатика",
                "Второй модуль",
                "Заряд, электрическое поле, закон Кулона и поляризация.",
                "В ФОКУСЕ",
                new[] { "Заряд", "Поле", "Кулон", "Поляризация" },
                0,
                "#00A6D6"),
            CreateTopic(
                "magnets",
                "Магниты",
                "Раздел",
                "Полюса, поле, линии индукции и взаимодействие магнитов.",
                "КАРКАС",
                new[] { "Полюса", "Поле", "Компас" },
                1,
                "#DE3F32"),
            CreateTopic(
                "electromagnets",
                "Электромагниты",
                "Раздел",
                "Катушки, сердечники, ток и управляемое магнитное поле.",
                "КАРКАС",
                new[] { "Катушка", "Сердечник", "Реле" },
                2,
                "#7A3FD1"),
            CreateTopic(
                "mechanics",
                "Механика",
                "Раздел",
                "Силы, равновесие, работа, энергия и простые механизмы.",
                "ПОЗЖЕ",
                new[] { "Сила", "Работа", "Энергия" },
                3,
                "#2F8F5B"),
            CreateTopic(
                "kinematics",
                "Кинематика",
                "Раздел",
                "Положение, скорость, ускорение и графики движения.",
                "ПОЗЖЕ",
                new[] { "Скорость", "Ускорение", "Траектория" },
                4,
                "#EF8B21"),
            CreateTopic(
                "waves",
                "Волны",
                "Будущий раздел",
                "Колебания, частота, амплитуда, резонанс и перенос энергии.",
                "ИДЕЯ",
                new[] { "Частота", "Амплитуда", "Резонанс" },
                5,
                "#1D63C9"),
            CreateTopic(
                "optics",
                "Оптика",
                "Будущий раздел",
                "Свет, отражение, преломление, линзы и простые лучевые схемы.",
                "ИДЕЯ",
                new[] { "Луч", "Линза", "Призма" },
                6,
                "#D8A313"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectrostaticsSections()
    {
        return new[]
        {
            CreateLearningOption(
                "theory",
                "Теория",
                "Электрический заряд, поле, закон Кулона и поляризация.",
                "УРОКИ",
                true,
                0,
                "#00A6D6"),
            CreateLearningOption(
                "cards",
                "Задания с карточками",
                "Карточки для запоминания знаков зарядов, обозначений и базовых правил.",
                "ПРАКТИКА",
                true,
                6,
                "#7A3FD1"),
            CreateLearningOption(
                "lab",
                "Мини-лаборатория",
                "Будущие опыты с зарядами, линиями поля и пробным зарядом.",
                "СИМУЛЯЦИЯ",
                true,
                2,
                "#2F8F5B"),
            CreateLearningOption(
                "reference",
                "Справочник",
                "Формулы, символы, единицы измерения и правила знаков.",
                "СПРАВКА",
                true,
                7,
                "#D8A313"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectricitySections()
    {
        return new[]
        {
            CreateLearningOption(
                "electricity-theory",
                "Теория",
                "Ток, напряжение, сопротивление, закон Ома и первые цепи.",
                "УРОКИ",
                true,
                5,
                "#1D63C9"),
            CreateLearningOption(
                "electricity-cards",
                "Задания с карточками",
                "Карточки для запоминания элементов цепи, обозначений и свойств соединений.",
                "ПРАКТИКА",
                true,
                6,
                "#7A3FD1"),
            CreateLearningOption(
                "electricity-lab",
                "Мини-лаборатория",
                "Каркас конструктора простых цепей с источником, ключом, лампой и резистором.",
                "СИМУЛЯЦИЯ",
                true,
                2,
                "#2F8F5B"),
            CreateLearningOption(
                "electricity-reference",
                "Справочник",
                "Формулы, символы, единицы измерения и быстрые подсказки по цепям.",
                "СПРАВКА",
                true,
                7,
                "#D8A313"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectrostaticsCardTaskGroups()
    {
        return new[]
        {
            CreateLearningOption(
                "charge-signs",
                "Знаки зарядов",
                "Плюс, минус, притяжение, отталкивание и направление линий поля.",
                "ПАМЯТЬ",
                true,
                0,
                "#00A6D6"),
            CreateLearningOption(
                "symbols",
                "Обозначения",
                "Что означают q, E, F, k, r и другие символы в электростатике.",
                "СИМВОЛЫ",
                true,
                6,
                "#7A3FD1"),
            CreateLearningOption(
                "field-knowledge",
                "Поле и сила",
                "Как поле связано с силой, пробным зарядом и законом Кулона.",
                "ТЕОРИЯ",
                true,
                2,
                "#2F8F5B"),
            CreateLearningOption(
                "practical-exam",
                "Практический экзамен",
                "Блок под три задания на время. Содержимое добавим после того, как ты скинешь задачи.",
                "ОЖИДАЕТ ЗАДАЧ",
                false,
                4,
                "#C64D72"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectricityCardTaskGroups()
    {
        return new[]
        {
            CreateLearningOption(
                "circuit-elements",
                "Элементы цепи",
                "Источник, ключ, лампа, резистор, проводник и измерительные приборы.",
                "ПАМЯТЬ",
                true,
                0,
                "#00A6D6"),
            CreateLearningOption(
                "electricity-symbols",
                "Обозначения",
                "I, U, R, ЭДС, P и другие символы, которые встречаются в задачах.",
                "СИМВОЛЫ",
                true,
                6,
                "#7A3FD1"),
            CreateLearningOption(
                "circuit-knowledge",
                "Знание цепей",
                "Последовательное, параллельное и смешанное соединение.",
                "ТЕОРИЯ",
                true,
                2,
                "#2F8F5B"),
            CreateLearningOption(
                "electricity-practical-exam",
                "Практический экзамен",
                "Блок под три задания на время. Наполним, когда ты скинешь задачи.",
                "ОЖИДАЕТ ЗАДАЧ",
                false,
                4,
                "#C64D72"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectrostaticsTheoryCards()
    {
        return new[]
        {
            CreateLearningOption("charge", "Электрический заряд", "Положительные и отрицательные заряды, взаимодействие и нейтральное тело.", "УРОК", true, 0, "#00A6D6"),
            CreateLearningOption("electric-field", "Электрическое поле", "Как заряд создает поле и почему направление показывают от плюса к минусу.", "СКОРО", true, 5, "#1D63C9"),
            CreateLearningOption("coulomb-law", "Закон Кулона", "Как сила зависит от зарядов, расстояния и знаков взаимодействующих тел.", "СКОРО", true, 2, "#7A3FD1"),
            CreateLearningOption("polarization-card", "Поляризация", "Смещение зарядов внутри нейтрального тела рядом с заряженным объектом.", "СКОРО", true, 7, "#C64D72"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectricityTheoryCards()
    {
        return new[]
        {
            CreateLearningOption("current", "Электрический ток", "Направленное движение зарядов, сила тока и условия существования тока.", "УРОК", true, 5, "#1D63C9"),
            CreateLearningOption("voltage", "Напряжение", "Что показывает U и почему источник энергии заставляет заряды двигаться.", "СКОРО", true, 2, "#7A3FD1"),
            CreateLearningOption("resistance", "Сопротивление", "Что такое R и как сопротивление влияет на ток в цепи.", "СКОРО", true, 7, "#C64D72"),
            CreateLearningOption("ohm-law-card", "Закон Ома", "Связь силы тока, напряжения и сопротивления на участке цепи.", "СКОРО", true, 0, "#00A6D6"),
        };
    }

    private static FormulaPart T(string text) => new() { Text = text };

    private static FormulaPart V(string text) => new() { Text = text, IsVector = true };

    private static IEnumerable<LessonPage> CreateChargeLessonPages()
    {
        return new[]
        {
            new LessonPage
            {
                Id = "charge-basics",
                Title = "Что такое электрический заряд",
                Subtitle = "Заряд - это свойство частиц и тел вступать в электрическое взаимодействие.",
                Body = "У тела может быть положительный заряд, отрицательный заряд или почти нейтральное состояние. В школьной физике заряд обычно обозначают буквой q и измеряют в кулонах: Кл. Заряд создает вокруг себя электрическое поле. У положительного заряда направление поля принято показывать наружу, а у отрицательного - внутрь, к самому заряду. Если тело нейтрально, это не значит, что внутри нет зарядов. Обычно это значит, что положительных и отрицательных зарядов примерно поровну.",
                FormulaTitle = "Заряд, элементарный заряд и поле",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Equation = "q = ±N · e",
                        Explanation = "N - целое число лишних или недостающих элементарных зарядов. Знак показывает, каких зарядов больше: положительных или отрицательных.",
                    },
                    new FormulaItem
                    {
                        Equation = "e = 1.602 · 10⁻¹⁹ Кл",
                        Explanation = "e - элементарный заряд: модуль заряда протона и электрона. У протона знак плюс, у электрона знак минус.",
                    },
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { V("E"), T(" =") },
                        NumeratorParts = new[] { V("F") },
                        DenominatorParts = new[] { T("q₀") },
                        Explanation = "Напряженность поля равна силе, действующей на маленький положительный пробный заряд q₀, деленной на величину этого заряда.",
                    },
                },
                KeyPoint = "Главная мысль: заряд бывает положительным и отрицательным, а вектор поля идет от плюса и к минусу.",
                ExampleTitle = "Пример",
                Example = "Если потереть пластиковую линейку о ткань, часть электронов может перейти с одного тела на другое. Линейка получит избыток электронов и станет заряженной отрицательно.",
                VisualKind = "kinds",
            },
            new LessonPage
            {
                Id = "charge-interaction",
                Title = "Как заряды взаимодействуют",
                Subtitle = "Одинаковые заряды отталкиваются, разные заряды притягиваются.",
                Body = "Электрическое взаимодействие может притягивать или отталкивать тела. Два положительных заряда отталкиваются. Два отрицательных заряда тоже отталкиваются. Положительный и отрицательный заряды притягиваются. Линии поля помогают увидеть направление: они выходят из положительного заряда и входят в отрицательный. Поэтому между плюсом и минусом линии как будто связывают заряды, а у одинаковых зарядов линии расходятся.",
                FormulaTitle = "Закон Кулона",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { T("F =") },
                        NumeratorParts = new[] { T("k · |q₁q₂|") },
                        DenominatorParts = new[] { T("r²") },
                        Explanation = "Это модуль силы взаимодействия двух точечных зарядов. Если расстояние r увеличить, сила быстро уменьшается, потому что r стоит в квадрате.",
                    },
                    new FormulaItem
                    {
                        Equation = "k ≈ 8.99 · 10⁹ Н·м²/Кл²",
                        Explanation = "k - коэффициент в законе Кулона для вакуума. Для школьных задач в воздухе обычно берут то же округление.",
                    },
                    new FormulaItem
                    {
                        Equation = "q₁q₂ > 0 → отталкивание;  q₁q₂ < 0 → притяжение",
                        Explanation = "Произведение зарядов удобно использовать как короткое правило знаков.",
                    },
                },
                KeyPoint = "Правило знаков: плюс с плюсом - отталкивание, минус с минусом - отталкивание, плюс с минусом - притяжение.",
                ExampleTitle = "Пример",
                Example = "Два одинаково заряженных шарика на нитях расходятся в стороны. Если один шарик положительный, а другой отрицательный, они отклоняются друг к другу.",
                VisualKind = "interaction",
            },
            new LessonPage
            {
                Id = "polarization",
                Title = "Поляризация: заряды смещаются внутри тела",
                Subtitle = "Даже нейтральное тело может стать полярным рядом с заряженным объектом.",
                Body = "Поляризация - это разделение зарядов внутри тела без обязательного изменения общего заряда тела. Если рядом появляется положительно заряженный шарик, подвижные отрицательные заряды в другом шарике смещаются ближе к нему, а положительные оказываются дальше. Если рядом отрицательный заряд, картина меняется наоборот. Для двух одинаково заряженных тел одноименные заряды стремятся уйти как можно дальше друг от друга, поэтому распределение заряда по поверхности становится неравномерным.",
                FormulaTitle = "Дипольный момент",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "parts",
                        Parts = new[] { V("p"), T(" = q · "), V("l") },
                        Explanation = "p - дипольный момент как вектор. Он показывает, насколько заметно положительный и отрицательный центры зарядов разошлись внутри тела.",
                    },
                    new FormulaItem
                    {
                        Kind = "parts",
                        Parts = new[] { T("|"), V("p"), T("| = q · l") },
                        Explanation = "Это модуль дипольного момента: q - модуль разделенного заряда, l - расстояние между центрами положительного и отрицательного зарядов.",
                    },
                },
                KeyPoint = "Поляризация не обязательно заряжает тело целиком: она перераспределяет плюсы и минусы внутри него.",
                ExampleTitle = "Пример",
                Example = "Заряженная палочка может притягивать маленькие нейтральные кусочки бумаги. Бумага в целом нейтральна, но ближняя сторона становится заряженной противоположно палочке, поэтому появляется притяжение.",
                VisualKind = "polarization",
            },
            new LessonPage
            {
                Id = "conductors-and-insulators",
                Title = "Проводники и изоляторы",
                Subtitle = "В одних телах заряды легко смещаются, в других почти закреплены.",
                Body = "В проводниках, например в металлах, часть электронов может свободно перемещаться по телу. Поэтому заряд в проводнике способен быстро перераспределяться. В изоляторах заряды связаны намного сильнее, поэтому им труднее двигаться через материал. Это важно для понимания проводов, ручек приборов и защитной изоляции.",
                FormulaTitle = "Поле в проводнике",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "parts",
                        Parts = new[] { V("E"), T("(внутри) = 0") },
                        Explanation = "В электростатическом равновесии поле внутри проводника компенсировано.",
                    },
                    new FormulaItem
                    {
                        Equation = "qизб. → поверхность",
                        Explanation = "Избыточный заряд в проводнике перераспределяется по поверхности, а не остается равномерно внутри объема.",
                    },
                },
                KeyPoint = "Проводник помогает зарядам двигаться. Изолятор мешает их свободному движению.",
                ExampleTitle = "Пример",
                Example = "Медный провод хорошо проводит ток, потому что в металле есть подвижные электроны. Пластиковая оболочка провода изолирует, чтобы заряд не уходил туда, куда не надо.",
                VisualKind = "conductor",
            },
            new LessonPage
            {
                Id = "charge-conservation",
                Title = "Сохранение заряда",
                Subtitle = "Заряд не появляется из ничего и не исчезает бесследно.",
                Body = "В замкнутой системе общий электрический заряд сохраняется. Он может переходить от одного тела к другому или перераспределяться внутри системы, но сумма зарядов остается той же. Это похоже на учет: если одно тело получило лишние электроны, другое тело обычно их потеряло.",
                FormulaTitle = "Закон сохранения заряда",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Equation = "Σq(до) = Σq(после)",
                        Explanation = "Если система замкнута, общий заряд до взаимодействия равен общему заряду после взаимодействия.",
                    },
                    new FormulaItem
                    {
                        Equation = "q₁ + q₂ + ... + qₙ = const",
                        Explanation = "Заряды могут перераспределяться, но их сумма остается постоянной.",
                    },
                },
                KeyPoint = "Закон сохранения заряда: общий заряд замкнутой системы не меняется.",
                ExampleTitle = "Пример",
                Example = "Если тело A получило заряд -3q, то какое-то другое тело или окружающая система потеряли такой же отрицательный заряд. Общий баланс не нарушается.",
                VisualKind = "conservation",
            },
        };
    }

    private static IEnumerable<LessonPage> CreateElectricCurrentLessonPages()
    {
        return new[]
        {
            new LessonPage
            {
                Id = "current-basics",
                Title = "Что такое электрический ток",
                Subtitle = "Ток - это направленное движение заряженных частиц.",
                Body = "В металлах ток связан с движением свободных электронов. В растворах и газах ток могут переносить ионы. Важно не просто наличие зарядов, а их упорядоченное движение в одну сторону. Если заряды движутся хаотично, как молекулы в теплом теле, электрического тока в цепи еще нет.",
                FormulaTitle = "Сила тока",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { T("I =") },
                        NumeratorParts = new[] { T("Δq") },
                        DenominatorParts = new[] { T("Δt") },
                        Explanation = "I показывает, какой заряд Δq проходит через поперечное сечение проводника за время Δt.",
                    },
                    new FormulaItem
                    {
                        Equation = "1 А = 1 Кл/с",
                        Explanation = "Один ампер означает, что за одну секунду через сечение проходит заряд один кулон.",
                    },
                },
                KeyPoint = "Главная мысль: ток появляется, когда заряды движутся направленно, а сила тока показывает скорость переноса заряда.",
                ExampleTitle = "Пример",
                Example = "Если через лампу за 2 секунды прошел заряд 6 Кл, то сила тока равна 3 А. Чем больше заряд проходит за то же время, тем больше ток.",
                VisualKind = "current-flow",
            },
            new LessonPage
            {
                Id = "closed-circuit",
                Title = "Почему цепь должна быть замкнутой",
                Subtitle = "Для длительного тока нужен источник энергии и замкнутый путь.",
                Body = "Источник создает условия для движения зарядов, но ток не пойдет через разрыв. Цепь должна иметь замкнутый путь: источник, проводники, потребитель и возвращение к источнику. Ключ удобен тем, что управляет этим путем: замкнул - ток идет, разомкнул - ток прекращается.",
                FormulaTitle = "Напряжение как энергия на заряд",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { T("U =") },
                        NumeratorParts = new[] { T("A") },
                        DenominatorParts = new[] { T("q") },
                        Explanation = "Напряжение U показывает, какую работу A поле или источник совершает при переносе заряда q.",
                    },
                    new FormulaItem
                    {
                        Equation = "1 В = 1 Дж/Кл",
                        Explanation = "Один вольт означает один джоуль энергии на один кулон заряда.",
                    },
                },
                KeyPoint = "Без замкнутого пути ток не течет, даже если источник подключен к одному концу провода.",
                ExampleTitle = "Пример",
                Example = "Лампа не светится при разомкнутом ключе, потому что у зарядов нет полного пути через цепь. Когда ключ замыкают, путь появляется и ток проходит через лампу.",
                VisualKind = "closed-circuit",
            },
            new LessonPage
            {
                Id = "voltage-resistance",
                Title = "Напряжение, сопротивление и закон Ома",
                Subtitle = "Напряжение помогает току течь, сопротивление ограничивает ток.",
                Body = "Напряжение можно воспринимать как причину, которая заставляет заряды двигаться по цепи. Сопротивление показывает, насколько элемент мешает этому движению. Если напряжение увеличить, ток обычно увеличится. Если сопротивление увеличить, ток станет меньше.",
                FormulaTitle = "Закон Ома для участка цепи",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { T("I =") },
                        NumeratorParts = new[] { T("U") },
                        DenominatorParts = new[] { T("R") },
                        Explanation = "При постоянном сопротивлении ток прямо пропорционален напряжению.",
                    },
                    new FormulaItem
                    {
                        Kind = "fractionParts",
                        LeftParts = new[] { T("R =") },
                        NumeratorParts = new[] { T("U") },
                        DenominatorParts = new[] { T("I") },
                        Explanation = "Так можно найти сопротивление участка, если известны напряжение и сила тока.",
                    },
                },
                KeyPoint = "Закон Ома связывает три главные величины электрических цепей: I, U и R.",
                ExampleTitle = "Пример",
                Example = "Если на резисторе напряжение 12 В, а сопротивление 6 Ом, то ток равен 2 А: I = 12 / 6.",
                VisualKind = "ohm-law",
            },
            new LessonPage
            {
                Id = "circuit-types",
                Title = "Последовательные и параллельные цепи",
                Subtitle = "В одной цепи может быть один путь для тока или несколько ветвей.",
                Body = "При последовательном соединении элементы стоят один за другим, поэтому через них проходит один и тот же ток. При параллельном соединении цепь делится на ветви: напряжение на ветвях одинаковое, а общий ток складывается из токов по ветвям.",
                FormulaTitle = "Базовые правила соединений",
                Formulas = new[]
                {
                    new FormulaItem
                    {
                        Equation = "Rпосл. = R₁ + R₂ + ...",
                        Explanation = "При последовательном соединении сопротивления складываются.",
                    },
                    new FormulaItem
                    {
                        Equation = "Iобщ. = I₁ + I₂ + ...",
                        Explanation = "При параллельном соединении общий ток равен сумме токов по ветвям.",
                    },
                },
                KeyPoint = "Последовательная цепь дает один путь для тока, параллельная - несколько независимых ветвей.",
                ExampleTitle = "Пример",
                Example = "Гирлянда может быть устроена так, что при перегорании одной лампы гаснет вся цепь. Это признак последовательного участка. Домашняя проводка устроена иначе: приборы работают параллельно.",
                VisualKind = "circuit-types",
            },
        };
    }

    private static IEnumerable<LearningOption> CreateLabCards()
    {
        return new[]
        {
            CreateLearningOption("field-lines-lab", "Линии поля", "Будущий опыт с направлением поля вокруг положительных и отрицательных зарядов.", "КАРКАС", true, 0, "#00A6D6"),
            CreateLearningOption("test-charge-lab", "Пробный заряд", "Проверка направления силы на маленький положительный заряд.", "КАРКАС", true, 2, "#2F8F5B"),
            CreateLearningOption("coulomb-lab", "Закон Кулона", "Опыт с расстоянием между зарядами и изменением силы взаимодействия.", "КАРКАС", true, 6, "#D8A313"),
            CreateLearningOption("polarization-lab", "Поляризация", "Смещение зарядов внутри нейтрального тела рядом с заряженным объектом.", "КАРКАС", true, 5, "#1D63C9"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectricityLabCards()
    {
        return new[]
        {
            CreateLearningOption("simple-circuit-lab", "Простая цепь", "Источник, ключ, лампа и замкнутый путь для тока.", "КАРКАС", true, 0, "#00A6D6"),
            CreateLearningOption("resistor-lab", "Резистор", "Будущий опыт с изменением сопротивления и наблюдением за током.", "КАРКАС", true, 2, "#2F8F5B"),
            CreateLearningOption("lamp-brightness-lab", "Яркость лампы", "Связь напряжения, сопротивления и мощности на простом примере.", "КАРКАС", true, 6, "#D8A313"),
            CreateLearningOption("measurements-lab", "Измерения", "Амперметр, вольтметр и первые измерения в учебной цепи.", "КАРКАС", true, 5, "#1D63C9"),
        };
    }

    private static IEnumerable<LearningOption> CreateReferenceCards()
    {
        return new[]
        {
            CreateLearningOption("symbols-ref", "Символы", "q, E, F, k, r и другие обозначения из электростатики.", "СПРАВКА", true, 6, "#7A3FD1"),
            CreateLearningOption("units-ref", "Единицы", "Кулоны, ньютоны, метры и единицы напряженности поля.", "СПРАВКА", true, 0, "#00A6D6"),
            CreateLearningOption("formulas-ref", "Формулы", "Закон Кулона, напряженность поля и сохранение заряда.", "СПРАВКА", true, 2, "#2F8F5B"),
            CreateLearningOption("sign-rules-ref", "Правила знаков", "Когда заряды притягиваются, отталкиваются и как направлено поле.", "СПРАВКА", true, 7, "#C64D72"),
        };
    }

    private static IEnumerable<LearningOption> CreateElectricityReferenceCards()
    {
        return new[]
        {
            CreateLearningOption("electricity-symbols-ref", "Символы", "I, U, R, epsilon, P и другие обозначения для цепей.", "СПРАВКА", true, 6, "#7A3FD1"),
            CreateLearningOption("electricity-units-ref", "Единицы", "Амперы, вольты, омы, кулоны, джоули и ватты.", "СПРАВКА", true, 0, "#00A6D6"),
            CreateLearningOption("electricity-formulas-ref", "Формулы", "Сила тока, закон Ома, мощность и работа тока.", "СПРАВКА", true, 2, "#2F8F5B"),
            CreateLearningOption("circuit-types-ref", "Типы цепей", "Последовательное, параллельное и смешанное соединение.", "СПРАВКА", true, 7, "#C64D72"),
        };
    }

    private static IEnumerable<LearningOption> CreateCardTaskDetailCards(string groupId)
    {
        return groupId switch
        {
            "charge-signs" => new[]
            {
                CreateLearningOption("positive-card", "Положительный заряд", "Что означает плюс и куда направлены линии поля.", "КАРТОЧКИ", true, 0, "#00A6D6"),
                CreateLearningOption("negative-card", "Отрицательный заряд", "Что означает минус и почему линии поля входят в заряд.", "КАРТОЧКИ", true, 7, "#D8A313"),
                CreateLearningOption("repulsion-card", "Отталкивание", "Почему одинаковые заряды расходятся друг от друга.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("attraction-card", "Притяжение", "Почему разные заряды тянутся друг к другу.", "КАРТОЧКИ", true, 6, "#7A3FD1"),
            },
            "symbols" => new[]
            {
                CreateLearningOption("q-symbol", "q", "Электрический заряд и его единица измерения.", "КАРТОЧКИ", true, 0, "#00A6D6"),
                CreateLearningOption("e-field-symbol", "E", "Напряженность электрического поля.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("f-symbol", "F", "Сила электрического взаимодействия.", "КАРТОЧКИ", true, 5, "#1D63C9"),
                CreateLearningOption("k-symbol", "k", "Коэффициент в законе Кулона.", "КАРТОЧКИ", true, 6, "#7A3FD1"),
            },
            "field-knowledge" => new[]
            {
                CreateLearningOption("field-direction", "Направление поля", "От плюса к минусу и по силе на положительный пробный заряд.", "КАРТОЧКИ", true, 0, "#00A6D6"),
                CreateLearningOption("coulomb-distance", "Расстояние r", "Почему сила уменьшается при увеличении расстояния.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("charge-product", "Произведение зарядов", "Как знак q1q2 помогает понять притяжение или отталкивание.", "КАРТОЧКИ", true, 6, "#7A3FD1"),
                CreateLearningOption("polarization-rule", "Поляризация", "Как заряды смещаются внутри нейтрального тела.", "КАРТОЧКИ", true, 7, "#C64D72"),
            },
            "circuit-elements" => new[]
            {
                CreateLearningOption("battery-card", "Источник", "Батарейка или источник питания создает напряжение.", "КАРТОЧКИ", true, 0, "#00A6D6"),
                CreateLearningOption("wire-card", "Проводник", "Путь, по которому заряды могут двигаться по цепи.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("switch-card", "Ключ", "Замыкает или размыкает цепь.", "КАРТОЧКИ", true, 5, "#1D63C9"),
                CreateLearningOption("lamp-card", "Лампа", "Потребитель энергии, который светится при протекании тока.", "КАРТОЧКИ", true, 7, "#D8A313"),
            },
            "electricity-symbols" => new[]
            {
                CreateLearningOption("i-symbol", "I", "Сила тока: сколько заряда проходит через сечение за секунду.", "КАРТОЧКИ", true, 5, "#1D63C9"),
                CreateLearningOption("u-symbol", "U", "Напряжение: энергия на единицу заряда.", "КАРТОЧКИ", true, 6, "#7A3FD1"),
                CreateLearningOption("r-symbol", "R", "Сопротивление: насколько элемент мешает току.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("p-symbol", "P", "Мощность: скорость потребления электрической энергии.", "КАРТОЧКИ", true, 7, "#C64D72"),
            },
            "circuit-knowledge" => new[]
            {
                CreateLearningOption("series-circuit", "Последовательная цепь", "Один путь для тока, одинаковая сила тока на элементах.", "КАРТОЧКИ", true, 0, "#00A6D6"),
                CreateLearningOption("parallel-circuit", "Параллельная цепь", "Несколько ветвей, одинаковое напряжение на ветвях.", "КАРТОЧКИ", true, 2, "#2F8F5B"),
                CreateLearningOption("mixed-circuit", "Смешанная цепь", "Комбинация последовательных и параллельных участков.", "КАРТОЧКИ", true, 6, "#7A3FD1"),
                CreateLearningOption("circuit-safety", "Короткое замыкание", "Опасный путь с очень малым сопротивлением.", "КАРТОЧКИ", true, 7, "#C64D72"),
            },
            _ => Array.Empty<LearningOption>(),
        };
    }

    private static Topic CreateTopic(
        string id,
        string title,
        string kicker,
        string description,
        string status,
        IReadOnlyList<string> modules,
        int atlasIndex,
        string accent)
    {
        var accentBrush = BrushFromHex(accent);

        return new Topic
        {
            Id = id,
            Title = title,
            Kicker = kicker,
            Description = description,
            Status = status,
            Modules = modules,
            TileViewbox = GetAtlasTile(atlasIndex),
            AccentBrush = accentBrush,
            ChipBrush = MixWithWhite(accentBrush.Color, 0.86),
            ChipTextBrush = MixWithBlack(accentBrush.Color, 0.42),
        };
    }

    private static LearningOption CreateLearningOption(
        string id,
        string title,
        string description,
        string status,
        bool isAvailable,
        int atlasIndex,
        string accent)
    {
        var accentBrush = BrushFromHex(accent);

        return new LearningOption
        {
            Id = id,
            Title = title,
            Description = description,
            Status = status,
            IsAvailable = isAvailable,
            TileViewbox = GetAtlasTile(atlasIndex),
            AccentBrush = accentBrush,
            ChipBrush = MixWithWhite(accentBrush.Color, 0.86),
            ChipTextBrush = MixWithBlack(accentBrush.Color, 0.42),
        };
    }

    private static Rect GetAtlasTile(int index)
    {
        const double columns = 4;
        const double rows = 2;

        var column = index % (int)columns;
        var row = index / (int)columns;

        return new Rect(column / columns, row / rows, 1 / columns, 1 / rows);
    }

    private static SolidColorBrush BrushFromHex(string hex)
    {
        var color = (Color)ColorConverter.ConvertFromString(hex);
        var brush = new SolidColorBrush(color);
        brush.Freeze();
        return brush;
    }

    private static SolidColorBrush MixWithWhite(Color color, double whiteShare)
    {
        return Mix(color, Colors.White, whiteShare);
    }

    private static SolidColorBrush MixWithBlack(Color color, double blackShare)
    {
        return Mix(color, Colors.Black, blackShare);
    }

    private static SolidColorBrush Mix(Color color, Color target, double targetShare)
    {
        var sourceShare = 1 - targetShare;
        var mixed = Color.FromRgb(
            (byte)((color.R * sourceShare) + (target.R * targetShare)),
            (byte)((color.G * sourceShare) + (target.G * targetShare)),
            (byte)((color.B * sourceShare) + (target.B * targetShare)));
        var brush = new SolidColorBrush(mixed);
        brush.Freeze();
        return brush;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private sealed record NavigationSnapshot(
        AppScreen Screen,
        string DetailTitle,
        string DetailDescription,
        IReadOnlyList<LearningOption> DetailCards,
        IReadOnlyList<LessonPage> LessonPages,
        int LessonPageIndex);
}

public enum AppScreen
{
    Home,
    ElectricityMenu,
    ElectrostaticsMenu,
    CardTasksMenu,
    DetailPage,
    ChargeLesson,
}
