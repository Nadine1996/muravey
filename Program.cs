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
        public bool NonFree;
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
        public int Index;
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
        public theBestSolution()
        {
            Way = new List<int>();
            Target = new Dictionary<int, int>();
        }

        public void setTheBestWay(Muravey muravey)
        {
            if (muravey.target.Count > Target.Count)
            {
                Way = muravey.Way;
                Target = muravey.target;
            }
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
        public const int COUNT_MURAVEY = 20;
        //коэффициент испарение феромона
        public const double EVAPORATION_COEF = 0.4;
        //коэффициент влияния феромона
        public const double ALPHA = 1;
        //коэффициент влияния эвристической функции
        public const double BETA = 1;
        public const double Q = 0.5;

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
        static void readInputfromFile()
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

            System.IO.StreamReader file =   new System.IO.StreamReader(@"D:\1.txt");
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
                        (list[i].Type == MACHINE.ROOT)
                        {
                            curves[i, j] = new Duga(1, 0.5);
                            curves[j, i] = new Duga(1, 0.5);
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
            solution = new theBestSolution();
            
            try
            {
                //заполнение списков vertex и  storage
                readInputfromFile();

                dugi= new Duga[vertex.Count, vertex.Count];
                //добавление дуг в графе 
                addCurves(vertex, dugi);
               
                //проход по графу и назначение виртуаьных элементов на физические
                Algoritm();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadKey();
        }

        //вычисление знаменателя в формуле вероятности (для выбора пути муравьем)
        public static double ZnamenatelP(List<Vertex> list, int i,BRANCH branch, MACHINE type, Duga[,] curves, Muravey muravey)
        {
            double sum = 0;
            for (int j = 0; j < list.Count; j++)
            {
                if (curves[i, j] != null && list[j].Type == type && list[j].Branch == branch && !muravey.Tabu.Contains(j))
                {
                    sum += curves[i, j].T * curves[i, j].N;
                }
            }
            return sum;
        }


        public static int startNode(List<Vertex> list, BRANCH branch)
        {
            int CurrentMashine = 0;
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Type == MACHINE.VIRTUAL && list[i].Branch == branch)
                {
                    CurrentMashine = list[i].Num;
                    break;
                }
            }
            return CurrentMashine;
        }
        
        //муравьиный алгорит
        public static void Algoritm()
        {

            List<int> ListVirtual_vertex = new List<int>();
            for(int i=0; i<vertex.Count; i++)
                if(vertex[i].Type == MACHINE.VIRTUAL && vertex[i].Branch ==BRANCH.CALCULATIVE)
                    ListVirtual_vertex.Add(vertex[i].Num);

            List<int> ListVirtual_storage = new List<int>();
            for (int i = 0; i < vertex.Count; i++)
                if (vertex[i].Type == MACHINE.VIRTUAL)
                    ListVirtual_storage.Add(vertex[i].Num);

            for (int m = 0; m < COUNT_MURAVEY; m++)
            {
                muravey[m] = new Muravey(0);
                antAlhoritm(vertex, muravey[m], ListVirtual_vertex, dugi, BRANCH.CALCULATIVE);

                Console.WriteLine();
                antAlhoritm(vertex, muravey[m], ListVirtual_storage, dugi, BRANCH.STORAGE);
                foreach (int i in muravey[m].Way)
                    Console.Write(i + "->");
                Console.WriteLine();
                solution.setTheBestWay(muravey[m]);
               
            }
            
            Console.ReadLine();
        }

        public static void antAlhoritm(List<Vertex> list, Muravey muravey, List<int> ListVirtual, Duga[,] curves, BRANCH branch)
        {
            int CurrentMashine = startNode(list,branch);
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
                    //  muravey.Way.Add(CurrentMashine);
                }

                //получение индекса вирт.машины, к которой переходит муравей (вирт ->вирт)
                CurrentMashine = roulettePath(list, CurrentMashine, branch, MACHINE.VIRTUAL, muravey, curves);
                //если все вирт.машины посещены,то возврат к исходному
                if (CurrentMashine == 0) { muravey.Way.Add(CurrentMashine); break; }
            }
            //изменение кол-ва ферромона на дугах
            updateFerromon(list, muravey, curves);
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
                if (curves[CurrentMashine, j] != null && list[j].Type == machine && list[j].Branch == branch)
                {
                    if ((machine == MACHINE.VIRTUAL && !muravey.Tabu.Contains(j)) || machine == MACHINE.REAL)
                    {
                        //верроятность перехода
                        p = curves[CurrentMashine, j].T * curves[CurrentMashine, j].N / ZnamenatelP(list,CurrentMashine,branch, machine, curves, muravey);
                    }
                    else
                    {
                        p = 0;
                    }
                    P.Add(j, p);
                }
            }

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

        //был ли переход из i-той вершины в j-тую
        public static bool vertexContains(Muravey muravey, int i, int j)
        {
            string Way = "";
            for(int w =0; w < muravey.Way.Count; w++)
            {
                Way += muravey.Way[w] + ", ";
            }
            if (Way.Contains(i + ", " + j)) return true;
            else return false;
        } 
        
        //вычисление дельта-тау для муравья
        public static double deltaTau(Muravey muravey, Duga[,] curves)
        {
            double sum = 0;
            
            for (int k = 1; k < (muravey.Way.Count - 1); k++)
            {
                int i = muravey.Way[k];
                int j = muravey.Way[k+1];
                if(vertexContains(muravey, i,j) && curves[i, j]!=null)
                    sum += curves[i, j].N * Q;
            }
            return sum;
        }

        //изменение кол-ва ферромона после прохождения муравья
        public static void updateFerromon(List<Vertex> list, Muravey muravey, Duga[,] curves)
        {
            double delta = deltaTau(muravey, curves);
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (curves[i, j] != null)
                    {
                        curves[i, j].T = (1 - EVAPORATION_COEF) * curves[i, j].T;
                        //если по дуге прошел муравей
                        if (vertexContains(muravey, i, j)) curves[i, j].T += delta;
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
