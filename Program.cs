using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace test_dict_select
{
    class Duga
    {
        public double T;
        public double N;
        public Duga(double t, double n)
        {
            T = t;
            N = n;
        }
    }

    class Vertex
    {
        public int Core;
        public BRANCH Branch;
        public MACHINE Type;
        public int Num;
        public Vertex(int c,BRANCH branch, MACHINE type, int num)
        {
            Core = c;
            Type = type;
            Num = num;
            Branch = branch;
        }
    }

    public enum MACHINE { VIRTUAL, REAL, ROOT };

    public enum BRANCH { CALCULATIVE , STORAGE, ROOT };

    class Muravey
    {
        //путь муравья
        public List<int> Way; 
        //посещенные вершины, запрещены для повторного посещения (вирт. машины)
        public List<int> Tabu;
        //соответствие вирт.машина-реальная машина
        public Dictionary<int, int> target;

        public Muravey(int first)
        {
            Way = new List<int>();
            Tabu = new List<int>();
            target = new Dictionary<int, int>();
            Way.Add(first);
        }
    }

    class theBestSolution
    {
        public List<int> Way;
        public Dictionary<int, int> Target;
        public double[,] AdjacencyMatrix;
        public theBestSolution(int vertex_count)
        {
            Way = new List<int>();
            Target = new Dictionary<int, int>();
            AdjacencyMatrix = new double[vertex_count, vertex_count];
        }

        public void setTheBestWay(Muravey muravey)
        {
            if (muravey.target.Count >= Target.Count)
            {
                Way = muravey.Way;
                Target = muravey.target;
            }
        }

        //оценка сходимости алгоритма по последним 10 муравьям
        public void estimateWay(Muravey [] muravey)
        {
            int[] count = new int [10];
            if (muravey.Length >= 10)
            {
                for (int i = muravey.Length-1; i > (muravey.Length - 11); i--)
                    for (int j = muravey.Length -1; j > (muravey.Length - 11); j--)
                    {
                        if (muravey[i].Way.SequenceEqual(muravey[j].Way))
                            count[muravey.Length-1-i]++;
                    }

                int max = 0;
                for(int i = 1; i<10; i++)
                {
                    if (count[i] > max) max = count[i];
                }
                this.Way = muravey[muravey.Length - 1 - max].Way;
                this.Target = muravey[muravey.Length - 1 - max].target;
            }
        }

        // Матрица смежности графа
        public void theAdjacencyMatrixOfAGraph(List<Vertex> list, Duga[,] curves)
        {
            AdjacencyMatrix = new double[list.Count, list.Count];
            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < list.Count; j++)
                    if (curves[i, j] != null && Program.vertexContains(this.Way, i, j))
                    {
                        AdjacencyMatrix[i, j] = 1;
                        AdjacencyMatrix[j, i] = 1;
                    }
            //for (int i = 0; i < list.Count; i++)
            //{
            //    for (int j = 0; j < list.Count; j++)
            //        Console.Write(AdjacencyMatrix[i, j] + " ");
            //    Console.Write("\n");
            //}

            for (int i = 0; i < list.Count; i++)
                for (int j = 0; j < i; j++)
                    if (AdjacencyMatrix[i, j] == 1)
                    {
                        if (i == 0 || j == 0)
                        {
                            AdjacencyMatrix[i, j] = 0.5;
                        }
                        else
                        {
                            AdjacencyMatrix[i, j] = summCore(list, i, j) + summCore(list, j, i);

                        }
                        AdjacencyMatrix[j, i] = AdjacencyMatrix[i, j];
                    }
            //for (int i = 0; i < list.Count; i++)
            //{
            //    for (int j = 0; j < list.Count; j++)
            //        Console.Write(AdjacencyMatrix[i, j] + " ");
            //    Console.Write("\n");
            //}
            Console.WriteLine();
        }

        //часть формулы для назначения веса дугам
        public double summCore(List<Vertex> list, int p, int q)
        {
            int summa = 0;
            for (int i = 0; i < list.Count; i++)
                if (this.AdjacencyMatrix[i, p] != 0 & i != q)
                    summa += list[i].Core;

            summa += list[q].Core;
            return (double)summa / (double)list[p].Core;
        }
    }

    class Program
    {
        //список вершин (вирт. машин +физ. вычислительных узлов)
        public static List<Vertex> vertex;
        //список вершин (storage-элементов +физ.хранилищ)
        public static List<Vertex> storage;
        //массив дуг
        public static Duga[,] dugi;
        public static theBestSolution solution;
       

        //массив муравьев
        public static Muravey[] muravey;
        //количество муравьев
        public const int COUNT_MURAVEY = 50;
        //коэффициент испарение феромона
        public const double EVAPORATION_COEF = 0.5;
        //коэффициент влияния феромона
        public const double ALPHA = 1;
        //коэффициент влияния эвристической функции
        public const double BETA = 1;
        public const double Q = 0.01;

        //"рулетка" для выбора пути муравью
        public static Random random = new Random();

        private static double Evristic(int i, int j, Muravey muravey){
            if (vertex[i].Type == MACHINE.VIRTUAL && vertex[j].Type == MACHINE.VIRTUAL)
            {
                int max = 0;
                for (int k = 0; k < vertex.Count; k++)
                {
                    if (vertex[k].Core > max)
                        max = vertex[k].Core;
                }
                double result = (double)vertex[j].Core / max;
                if (result > 1) return 0;
                else
                    return result;
            }
            if ((vertex[i].Type == MACHINE.VIRTUAL && vertex[j].Type == MACHINE.REAL)||
                (vertex[j].Type == MACHINE.VIRTUAL && vertex[i].Type == MACHINE.REAL))
            {
                int sum = 0;
                if (muravey.target.ContainsValue(vertex[j].Num))
                {
                    var keys = muravey.target.Where(x => x.Value == vertex[j].Num).ToList();

                    foreach (var k in keys)
                    {
                        sum += vertex[(int)k.Key].Core;
                    }
                }
                double result = (double)(sum + vertex[i].Core) / (double)vertex[j].Core;
                if(result>1) return 0;
                else return result;
            }
            throw new Exception("Машины имеют не допустимый тип: " + i.ToString() + ", " + j.ToString());
        }

        //назначение дугам первоначальных значений эвристической ф-ии после прохода каждого муравья
        private static void restartEvristic(List<Vertex> list, Duga[,] curves)
        {
            Muravey muravey = new Muravey(0);
            for(int i = 1; i< list.Count; i++)
                for(int j = 1; j< list.Count; j++)
                {
                    if(curves[i, j]!=null)
                        curves[i, j].N = Evristic(i, j, muravey);
                }
        }

        //считывание входных данных из файла
        static void readInputfromFile(string filePath)
        {
            //промежуточная строка для построчного считывания из файла 
            string line;
            //счетчики количества машин 
            //1--> вирт.машины 
            //2--> физ.вычислительные узлы 
            //3--> storage-элементы 
            //4--> физ.хранилища
            //int counter_virtual = 0;
            //int counter_storage = 0;
            int count = 0;

            System.IO.StreamReader file =   new System.IO.StreamReader(@filePath);
            while ((line = file.ReadLine()) != null)
            {
                String[] substrings = line.Split(' ');
                if(substrings.Length == 2)
                {
                    switch (substrings[0])
                    {
                        case "1":
                            count++;
                            vertex.Add(new Vertex(Convert.ToInt32(substrings[1]),BRANCH.CALCULATIVE, MACHINE.VIRTUAL, count));
                            break;

                        case "2":
                            count++;
                            vertex.Add(new Vertex(Convert.ToInt32(substrings[1]), BRANCH.CALCULATIVE, MACHINE.REAL, count));
                            break;

                        case "3":
                            count++;
                            vertex.Add(new Vertex(Convert.ToInt32(substrings[1]), BRANCH.STORAGE, MACHINE.VIRTUAL, count));
                            break;

                        case "4":
                            count++;
                            vertex.Add(new Vertex(Convert.ToInt32(substrings[1]), BRANCH.STORAGE, MACHINE.REAL, count));
                            break;

                        default:
                            break;
                    }
                }
            
            }

            file.Close();
        }

        //добавление дуг в графе
        static void addCurves(List<Vertex> list, Duga[,]  curves)
        {            
            Muravey muravey = new Muravey(0);
            for (int i = 0; i < list.Count-1; i++)
            {
                for (int j = i+1; j < list.Count; j++)
                {
                    if (
                        (list[i].Type == MACHINE.VIRTUAL && list[j].Type == MACHINE.VIRTUAL && list[i].Branch == BRANCH.CALCULATIVE && list[j].Branch == BRANCH.CALCULATIVE ) ||
                        (list[i].Type == MACHINE.VIRTUAL && list[j].Type == MACHINE.REAL   && list[i].Branch == BRANCH.CALCULATIVE && list[j].Branch == BRANCH.CALCULATIVE)  ||
                        (list[i].Type == MACHINE.VIRTUAL && list[j].Type == MACHINE.VIRTUAL && list[i].Branch == BRANCH.STORAGE && list[j].Branch == BRANCH.STORAGE) ||
                        (list[i].Type == MACHINE.VIRTUAL && list[j].Type == MACHINE.REAL && list[i].Branch == BRANCH.STORAGE && list[j].Branch == BRANCH.STORAGE))
                    {
                        curves[i, j] = new Duga(1, Evristic(i, j, muravey));
                        curves[j, i] = new Duga(1, Evristic(j, i, muravey));
                    }
                    if
                        (list[i].Type == MACHINE.ROOT && list[j].Type == MACHINE.VIRTUAL)
                        {
                            curves[i, j] = new Duga(1, 1);
                            curves[j, i] = new Duga(1, 1);
                        }
                }
            }
        }

        static void Main(string[] args)
        {
            
            vertex = new List<Vertex>();
            vertex.Add(new Vertex(0, BRANCH.ROOT, MACHINE.ROOT, 0));
            storage = new List<Vertex>();
            storage.Add(new Vertex(0,BRANCH.ROOT, MACHINE.ROOT, 0));
            muravey = new Muravey[COUNT_MURAVEY];
            
            
            try
            {
                //заполнение списков vertex и  storage
                readInputfromFile("D:/1.txt");

                dugi= new Duga[vertex.Count, vertex.Count];
                solution = new theBestSolution(vertex.Count);
                //добавление дуг в графе 
                addCurves(vertex, dugi);
               
                //проход по графу и назначение виртуаьных элементов на физические
                Algoritm();
                Deigstra(vertex, solution.AdjacencyMatrix);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
           
            Console.ReadKey();
        }

        //алгоритм Дейкстры
        public static void Deigstra(List<Vertex> list, double[,] a)
        {
            int VERTEXES = list.Count;                  //Число вершин в графе
            int v = 0;
            int infinity = 1000;                        // Бесконечность
            int p = VERTEXES;                           // Количество вершин в графе

            // Будем искать путь из вершины s в вершину g
            int s;                                      // Номер исходной вершины
            int g;                                      // Номер конечной вершины
            Console.WriteLine("Введите s: ");           // Номер может изменяться от 0 до p-1
            s = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("Введите g: ");
            g = Convert.ToInt32(Console.ReadLine());
            int[] x = new int[VERTEXES];                //Массив, содержащий единицы и нули для каждой вершины,
                                                        // x[i]=0 - еще не найден кратчайший путь в i-ю вершину,
                                                        // x[i]=1 - кратчайший путь в i-ю вершину уже найден
            double [] t = new double[VERTEXES];         //t[i] - длина кратчайшего пути от вершины s в i
            int[] h = new int[VERTEXES];                //h[i] - вершина, предшествующая i-й вершине на кратчайшем пути

            // Инициализируем начальные значения массивов
            int u;                                      // Счетчик вершин
            for (u = 0; u < p; u++)
            {
                t[u] = infinity;                        //Сначала все кратчайшие пути из s в i равны бесконечности
                x[u] = 0;                               // и нет кратчайшего пути ни для одной вершины
            }
            h[s] = 0;                                   // s - начало пути, поэтому этой вершине ничего не предшествует
            t[s] = 0;                                   // Кратчайший путь из s в s равен 0
            x[s] = 1;                                   // Для вершины s найден кратчайший путь
            v = s;                                      // Делаем s текущей вершиной

            while (true)
            {
                // Перебираем все вершины, смежные v, и ищем для них кратчайший путь
                for (u = 0; u < p; u++)
                {
                    if (a[v, u] == 0) continue;                // Вершины u и v несмежные
                    if (x[u] == 0 && t[u] > t[v] + a[v, u])    //Если для вершины u еще не найден кратчайший путь и новый путь в u короче чем старый, то
                    {
                        t[u] = t[v] + a[v, u];                 //запоминаем более короткую длину пути в массив t и
                        h[u] = v;                              //запоминаем, что v->u часть кратчайшего пути из s->u
                    }
                }

                // Ищем из всех длин некратчайших путей самый короткий
                double w = infinity;                           // Для поиска самого короткого пути
                v = -1;                                        // В конце поиска v - вершина, в которую будет найден новый кратчайший путь. Она станет текущей вершиной
                for (u = 0; u < p; u++)                        // Перебираем все вершины.
                {
                    if (x[u] == 0 && t[u] < w)                 // Если для вершины не найден кратчайший путь и если длина пути в вершину u меньше уже найденной, то
                    {
                        v = u;                                 // текущей вершиной становится u-я вершина
                        w = t[u];
                    }
                }
                if (v == -1)
                {
                    Console.WriteLine("Нет пути из вершины " + s + " в вершину " + g + ".");
                    break;
                }
                if (v == g)                                    // Найден кратчайший путь, выводим его
                {        
                    Console.Write("Кратчайший путь из вершины " + s + " в вершину " + g + ":");
                    u = g;
                    while (u != s)
                    {
                        Console.Write(" " + u);
                        u = h[u];
                    }
                    Console.Write(" " + s + ". Длина пути - " + t[g]);
                    break;
                }
                x[v] = 1;
            }
            Console.ReadLine();
        }

        //вычисление знаменателя в формуле вероятности (для выбора пути муравьем)
        public static double ZnamenatelP(List<Vertex> list, int i,BRANCH branch, MACHINE type, Duga[,] curves, Muravey muravey)
        {
            double sum = 0;
            for (int j = 0; j < list.Count; j++)
            {
                if (curves[i, j] != null && list[j].Type == type && list[j].Branch == branch && !muravey.Tabu.Contains(j))
                {
                    sum += Math.Pow(curves[i, j].T,ALPHA) * Math.Pow(curves[i, j].N,BETA);
                }
            }
            return sum;
        }

        //вычисление знаменателя в формуле вероятности (для выбора стартового вирт. элемента муравьем)
        public static double ZnamenatelStart(List<Vertex> list, BRANCH branch, Duga[,] curves)
        {
            MACHINE type = MACHINE.VIRTUAL;
            double sum = 0;
            for (int j = 0; j < list.Count; j++)
            {
                if (curves[0, j] != null && list[j].Type == type && list[j].Branch == branch)
                {
                    sum += Math.Pow(curves[0, j].T,ALPHA) * Math.Pow(curves[0, j].N,BETA);
                }
            }
            return sum;
        }

        //выбор стартового вирт. элемента муравьем
        public static int startNode(List<Vertex> list, Duga[,] curves, BRANCH branch)
        {
            int CurrentMashine = 0;
            Dictionary<int, double> P = new Dictionary<int, double>();
            double p;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == MACHINE.VIRTUAL && list[i].Branch == branch)
                {
                    //CurrentMashine = list[i].Num;
                    //верроятность перехода
                    p = Math.Pow(curves[CurrentMashine, i].T,ALPHA) * Math.Pow(curves[CurrentMashine, i].N,BETA) / ZnamenatelStart(list, branch, curves);
                    P.Add(i, p);
                }
            }

            return CurrentMashine = rouletteChouse(P);
        }
        
        //муравьиный алгорит
        public static void Algoritm()
        {
            //список вирт машин
            List<int> ListVirtual_vertex = new List<int>();
            for(int i=0; i<vertex.Count; i++)
                if(vertex[i].Type == MACHINE.VIRTUAL && vertex[i].Branch ==BRANCH.CALCULATIVE)
                    ListVirtual_vertex.Add(vertex[i].Num);

            //список вирт.элементов во всем графе
            List<int> ListVirtual_storage = new List<int>();
            for (int i = 0; i < vertex.Count; i++)
                if (vertex[i].Type == MACHINE.VIRTUAL)
                    ListVirtual_storage.Add(vertex[i].Num);

            //проход муравьями графа
            for (int m = 0; m < COUNT_MURAVEY; m++)
            {
                muravey[m] = new Muravey(0);
                //обход левой части графа(вирт.машины+физич.узлы)
                antAlhoritm(vertex, muravey[m], ListVirtual_vertex, dugi, BRANCH.CALCULATIVE);

                Console.WriteLine();
                //обход правой части графа(storage-элементы +хранилища данных)
                antAlhoritm(vertex, muravey[m], ListVirtual_storage, dugi, BRANCH.STORAGE);

                //изменение кол-ва ферромона на дугах
                updateFerromon(vertex, muravey[m], dugi);

                foreach (int i in muravey[m].Way)
                    Console.Write(i + "->");
                Console.WriteLine();

                //сортировка списка соответствий по ключу
                muravey[m].target = muravey[m].target.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key, pair => pair.Value);
                solution.setTheBestWay(muravey[m]);
            }
            //оценка сходимости алгоритма
            solution.estimateWay(muravey);
            solution.theAdjacencyMatrixOfAGraph(vertex, dugi);
        }

        public static void antAlhoritm(List<Vertex> list, Muravey muravey, List<int> ListVirtual, Duga[,] curves, BRANCH branch)
        {
            int CurrentMashine = startNode(list, curves, branch);
            //перезапись в первоначальные значения эвристической ф-ии
            restartEvristic(list, curves);

            while (!muravey.Tabu.SequenceEqual(ListVirtual))
            {
                //добавление текущей вершины к пути муравья
                muravey.Way.Add(CurrentMashine);
                //добавление текущей вершины в список табу для муравья
                muravey.Tabu.Add(CurrentMashine);

                //получение следующей физической вершины (вирт ->физ)
                int indVertex = roulettePath(list, CurrentMashine,branch, MACHINE.REAL, muravey, curves);
                if (indVertex != 0)
                {
                    //добавление вершины к пути муравья
                    muravey.Way.Add(indVertex);
                    //присвоение вирт машины физической
                    addTarget(list, CurrentMashine, indVertex, muravey, curves);
                    //добавление вершины к пути муравья(возврат к вирт. машине)
                      muravey.Way.Add(CurrentMashine);
                }

                //получение индекса вирт.машины, к которой переходит муравей (вирт ->вирт)
                CurrentMashine = roulettePath(list, CurrentMashine, branch, MACHINE.VIRTUAL, muravey, curves);
                //если все вирт.машины посещены,то возврат к исходному
                if (CurrentMashine == 0) { muravey.Way.Add(CurrentMashine); break; }
            }
            

        }

        //выбор дальнейшего пути из вероятностей
        public static int rouletteChouse(Dictionary<int, double> P)
        {
            int indVertex = 0;
            //рулетка
            double roulette = random.NextDouble();
            double rouletteSum = 0;
            foreach (KeyValuePair<int, double> kvp in P)
            {
                rouletteSum += kvp.Value;
                if (rouletteSum > roulette)
                {
                    indVertex = kvp.Key;
                    break;
                }
            }
            return indVertex;
        }

        public static int roulettePath(List<Vertex> list, int CurrentMashine,BRANCH branch, MACHINE machine, Muravey muravey, Duga[,] curves)
        {
            //Dictionary для хранения вероятностей перехода  вирт--->физ 
            Dictionary<int, double> P = new Dictionary<int, double>();
            int indVertex = 0;
            double p;

            //находнеие вероятностей перехода
            for (int j = 0; j < list.Count; j++)
            {
                //существут дуга && соотв. тип (вирт./физ.) && соответствут ветка (вычислит./storage)
                if (curves[CurrentMashine, j] != null &&  (list[j].Type == machine && list[j].Branch == branch) )
                {
                    if ((machine == MACHINE.VIRTUAL && !muravey.Tabu.Contains(j)) || machine == MACHINE.REAL)
                    {
                        //верроятность перехода
                        p = Math.Pow(curves[CurrentMashine, j].T,ALPHA) * Math.Pow(curves[CurrentMashine, j].N,BETA) / ZnamenatelP(list,CurrentMashine,branch, machine, curves, muravey);
                    }
                    else
                    {
                        p = 0;
                    }
                    P.Add(j, p);
                }
            }

          return indVertex = rouletteChouse(P);
        }

        //был ли переход из i-той вершины в j-тую
        public static bool vertexContains(List<int> Way, int i, int j)
        {
            string st = "";
            for(int w =0; w < Way.Count; w++)
            {
                st += Way[w] + ", ";
            }
            if (st.IndexOf(i + ", " + j + ", ")!=-1) return true;
            else return false;
        } 
        
        //вычисление дельта-тау для муравья
        public static double deltaTau(Muravey muravey, Duga[,] curves)
        {
            double sum = 0;
            
            for (int k = 0; k < (muravey.Way.Count - 1); k++)
            {
                int i = muravey.Way[k];
                int j = muravey.Way[k+1];
                if(vertexContains(muravey.Way, i,j) && curves[i, j]!=null)
                    sum += curves[i, j].N * Q;
            }
            return sum;
        }

        //изменение кол-ва ферромона после прохождения муравья
        public static void updateFerromon(List<Vertex> list, Muravey muravey, Duga[,] curves)
        {
            double delta = deltaTau(muravey, curves);
            int count = 0;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (curves[i, j] != null)
                    {
                        curves[i, j].T = (1 - EVAPORATION_COEF) * curves[i, j].T;
                        //если по дуге прошел муравей
                        if (vertexContains(muravey.Way, i, j))
                        {
                            curves[i, j].T += delta;
                            if (i == 0)
                                count++;
                        }
                            
                    }
                }
            }
        }

        //назначение вирт.машины физической для муравья и пересчет эвристической ф-ии 
        public static void addTarget(List<Vertex> list, int CurrentMashine, int RealMachine, Muravey muravey, Duga[,] curves)
        {
            muravey.target.Add(CurrentMashine, RealMachine);
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == MACHINE.VIRTUAL && curves[i, RealMachine] != null)
                {
                    curves[i, RealMachine].N = Evristic(i, RealMachine, muravey);
                }
            }
        }


    }
}
