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
        public MACHINE Type;
        public int Num;
        public Vertex(int c, MACHINE type, int num)
        {
            Core = c;
            Type = type;
            Num = num;
        }
    }

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

    public enum MACHINE {VIRTUAL, REAL, ROOT};

    class Program
    {
        public static List<Vertex> vertex;
        public static Duga[,] dugi;
        
        public static Muravey[] muravey;
        public const int COUNT_MURAVEY = 1;

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
                else return result;
            }
            if (vertex[i].Type == MACHINE.VIRTUAL && vertex[j].Type == MACHINE.REAL)
            {
                int sum = 0;
                if (muravey.target.ContainsValue(vertex[j].Num))
                {
                    var keys = muravey.target.Where(x => x.Value == vertex[j].Num).ToList();

                    foreach (var k in keys)
                    {
                        sum += vertex[(int)k.Value].Core;
                    }
                }
                double result = (double)(sum + vertex[i].Core) / (double)vertex[j].Core;
                if(result>1) return 0;
                else return result;
            }
            
            throw new Exception("Машины имеют не допустимый тип: " + i.ToString() + ", " + j.ToString());
        }

        static void Main(string[] args)
        {
            
            vertex = new List<Vertex>();
            vertex.Add(new Vertex(0, MACHINE.ROOT, 0));
            vertex.Add(new Vertex(2, MACHINE.VIRTUAL, 1));
            vertex.Add(new Vertex(4, MACHINE.VIRTUAL, 2));
            vertex.Add(new Vertex(4, MACHINE.REAL, 3));
            vertex.Add(new Vertex(2, MACHINE.REAL, 4));

            muravey = new Muravey[COUNT_MURAVEY];
            muravey[0] = new Muravey(0);
            try
            {
                dugi = new Duga[vertex.Count+1, vertex.Count+1];
                dugi[1, 3] = new Duga(1, Evristic(1, 3, muravey[0]));
                dugi[1, 4] = new Duga(1, Evristic(1, 4, muravey[0]));
                dugi[2, 3] = new Duga(1, Evristic(2, 3, muravey[0]));
                dugi[2, 4] = new Duga(1, Evristic(2, 4, muravey[0]));

                dugi[1, 2] = new Duga(1, Evristic(1, 2, muravey[0]));
                dugi[2, 1] = new Duga(1, Evristic(2, 1, muravey[0]));
            
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            int CurrentMashine = 0;
            for (int i = 0; i < vertex.Count; i++)
            {
                if (vertex[i].Type == MACHINE.VIRTUAL)
                {
                    CurrentMashine = vertex[i].Num;
                    break;
                }
            }
            Algoritm(CurrentMashine);
           
            Console.ReadKey();
        }

        public static double ZnamenatelP(int i, MACHINE type)
        {
            double sum = 0;
            for (int j = 0; j <= vertex.Count; j++)
            {
                if (dugi[i, j] != null && vertex[j].Type == type)
                {
                    sum += dugi[i, j].T * dugi[i, j].N;
                }
            }
            return sum;
        }

        public static int foundNewStartMachine(Muravey muravey, int CurrentMashine)
        {
            Dictionary<int, double> P = new Dictionary<int, double>();
            double maxP = -1, p = 0; int indMax = -1;
            for (int j = 0; j < vertex.Count; j++)
            {
                if (vertex[j].Type == MACHINE.VIRTUAL && dugi[CurrentMashine, j] != null)
                {
                    if (!muravey.Tabu.Contains(j))
                    {
                        p = dugi[CurrentMashine, j].T * dugi[CurrentMashine, j].N / ZnamenatelP(CurrentMashine, MACHINE.VIRTUAL);
                    }
                    else
                    {
                        p = 0;
                    }
                    if (p > maxP)
                    {
                        maxP = p;
                        indMax = j;
                    }
                    P.Add(j, p);
                }
            }
            //если все вирт. машины посещены,то переход на вершину 0
            if(maxP == 0) return 0;
            else  return indMax;
        }

        public static void Algoritm(int CurrentMashine)
        {
           
            List<int> ListVirtual = new List<int>();
            for(int i=0; i<vertex.Count; i++)
                if(vertex[i].Type == MACHINE.VIRTUAL)
                    ListVirtual.Add(vertex[i].Num);

            for (int m = 0; m < COUNT_MURAVEY; m++)
            {
                if(m!=0)  muravey[m] = new Muravey(0);
                while (!muravey[m].Tabu.SequenceEqual(ListVirtual))
                {
                    muravey[m].Way.Add(CurrentMashine);
                    muravey[m].Tabu.Add(CurrentMashine);
                    Dictionary<int, double> P = new Dictionary<int, double>();
                    double maxP = 0; int indMax = 0;
                    for (int j = 0; j <= vertex.Count; j++)
                    {
                        if (dugi[CurrentMashine, j] != null && vertex[j].Type == MACHINE.REAL)
                        {
                            double p = dugi[CurrentMashine, j].T * dugi[CurrentMashine, j].N / ZnamenatelP(CurrentMashine, MACHINE.REAL);
                            if (p > maxP)
                            {
                                maxP = p;
                                indMax = j;
                            }
                            P.Add(j, p);
                        }
                    }
                    muravey[m].Way.Add(indMax);
                    addTarget(CurrentMashine, indMax, muravey[m]);
                    muravey[m].Way.Add(CurrentMashine);
                    CurrentMashine = foundNewStartMachine(muravey[m], CurrentMashine);
                    if(CurrentMashine ==0) muravey[m].Way.Add(CurrentMashine);
                    Console.WriteLine("");
                }
            }
        }

        public static void addTarget(int CurrentMashine, int RealMachine, Muravey muravey)
        {
            muravey.target.Add(CurrentMashine, RealMachine);
            for (int i = 0; i < vertex.Count; i++)
            {
                if (vertex[i].Type == MACHINE.VIRTUAL && dugi[i, RealMachine] != null)
                {
                    dugi[i, RealMachine].N = Evristic(i, RealMachine, muravey);
                }
            }
        }
    }
}
