using System;
using System.Collections.Generic;
using System.IO;


namespace graphw1
{
    class Graph
    {
        bool orientation;
        List<Vertex> vertices;

        public Graph() //Конструктор без параметров
        {
            orientation = false;
            vertices = new List<Vertex>();
        }
        public Graph(int num, bool orientation) //Конструктор, принимающий кол-во вершин и ориентированность графа
        {
            vertices = new List<Vertex>();
            this.orientation = orientation;
            for (int i = 0; i < num; i++) //Добавление вершин
                AddVertex(i + 1);
        }
        public Graph(bool _d, List<Vertex> _vertices) //Служебный, для копии графа
        {
            orientation = _d;
            vertices = new List<Vertex>();
            foreach (Vertex vertex in _vertices)
                AddVertex(vertex.num);
            for (int i = 0; i < _vertices.Count; i++)
            {
                for (int j = i + 1; j < _vertices.Count; j++)
                {
                    Edge findedEdge = _vertices[i].checkEdge(_vertices[j]);
                    if (findedEdge != null)
                        AddEdge(findedEdge.fst.num, findedEdge.snd.num, findedEdge.weight);
                }
            }
        }
        public Graph(StreamReader fin, int TS) //Конструктор, принимающий поток ввода
        {
            vertices = new List<Vertex>();
            if (TS==1) //Заполнение графа из матрицы весов двудольного графа
            {
                orientation = false;
                int num_share1, num_share2; //Размер первой и второй доли графа
                string[] temp = fin.ReadLine().Trim().Split(" ");
                num_share1 = int.Parse(temp[0]); num_share2 = int.Parse(temp[1]);

                if ((num_share1 < 1) || (num_share2 < 1))  throw new ErrReadTS("Ошибка: указано некорретное кол-во вершин...");
                for (int i = 0; i < num_share1 + num_share2; i++)  AddVertex(i + 1); //Добавление вершин

                int[,] matrix = new int[num_share1, num_share2];
                for (int i = 0; i < num_share1; i++) //Заполнение матрицы
                {
                    string[] row = fin.ReadLine().Trim().Split(" ");
                    for (int j = 0; j < num_share2; j++)
                    {
                        int weight;
                        weight = int.Parse(row[j]);
                        matrix[i, j] = weight;
                    }
                }
                for (int i = 0; i < num_share1; i++) //Добавление рёбер с использованием матрицы
                    for (int j = 0; j < num_share2; j++)
                        if (matrix[i, j] > 0) AddEdge(i + 1, num_share1 + j + 1, matrix[i, j]);
            }
            else //Заполнение графа из перечисления множеств
            {
                string[] str = fin.ReadLine().Trim().Split(" ");
                if (str[1] == "1") orientation = true;
                else orientation = false;
                for (int i = 0; i < int.Parse(str[0]); i++) AddVertex(i + 1); //Добавление вершин

                while (!fin.EndOfStream) //Добавление рёбер
                {
                    string[] edge = fin.ReadLine().Split(" ");
                    AddEdge(int.Parse(edge[0]), int.Parse(edge[1]), int.Parse(edge[2]));
                }
            }
            fin.Close();
        }
        public void AddVertex(int num) //Добавление вершины
        {
            if (num < 1) throw new AttAddIncorrectVertex("Ошибка: попытка добавить неккоретную вершину...");
            else if (vertices.Find(vertex => vertex.num == num) != null) throw new AttAddExistingVertex("Ошибка: попытка добавить существующую вершину...");
            vertices.Add(new Vertex(num));
        }
        public void AddEdge(int fst, int snd, int weight) //Добавление ребра
        {
            Vertex vertex_1 = vertices.Find(vertex => vertex.num == fst);
            Vertex vertex_2 = vertices.Find(vertex => vertex.num == snd);
            if (vertex_1 == null || vertex_2 == null) throw new NonFindVertex("Ошибка: были указаны несуществующие вершины...");

            vertex_1.addEdge(weight, vertex_1, vertex_2);
            if (!orientation) vertex_2.addEdge(weight, vertex_2, vertex_1);
        }
        public void DeleteVertex(int num) //Удаление вершины
        {
            Vertex deleted_vertex = vertices.Find(deleted_vertex => deleted_vertex.num == num);
            if (deleted_vertex == null) throw new AttDelInixistingVertex("Ошибка: попытка удалить несуществующую вершину...");
            foreach (var vertex in vertices) //Удаление всех ребёр, ведущих в вершину, которую необходимо удалить 
                if (vertex.checkEdge(deleted_vertex) != null) vertex.deleteEdge(deleted_vertex);
            vertices.Remove(deleted_vertex);
        }
        public void DeleteEdge(int fst, int snd) //Удаление ребра
        {
            Vertex vertex_1 = vertices.Find(vertex => vertex.num == fst);
            Vertex vertex_2 = vertices.Find(vertex => vertex.num == snd);
            if (vertex_1 == null || vertex_2 == null) throw new AttDelInixistingEdge("Ошибка: попытка удалить несуществующее ребро...");
            vertex_1.deleteEdge(vertex_2);
            if (!orientation) vertex_2.deleteEdge(vertex_1);
        }
        public void Show1() //Перечисление множеств V и E графа
        {
            Console.WriteLine("Вершины: ");
            foreach (var vertex in vertices)
                Console.Write(vertex.num + " ");

            Console.WriteLine("\nРёбра: ");
            foreach (var vertex in vertices) Console.Write(vertex.printEdge1());

        }
        public void Show2() //В виде матрицы смежности / матрицы весов
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                foreach (var vertex in vertices)
                {
                    Edge edge = vertices[i].checkEdge(vertex);
                    if (edge == null)
                        Console.Write("{0,5}", 0 + " ");
                    else
                        Console.Write("{0,5}", edge.weight + " ");
                }
                Console.WriteLine();
            }
        }
        public void Show3() //В виде списков смежности 
        {
            foreach (var vertex in vertices)
            {
                Console.Write(vertex.num + "\n");
                string temp = vertex.printEdge2();
                Console.Write(temp);
            }
        }
        public void Show4() //В виде матрицы весов двудольного графа
        {
            List<Vertex> share1 = new List<Vertex>();
            List<Vertex> share2 = new List<Vertex>();
            if(!GetGraphShares(ref share1, ref share2)) throw new NonTS("Ошибка: граф не является двудольным..."); //Разбиение графа на две дволи

            for (int i = 0; i < share2.Count - 1; i++) //Сортировка по возрастанию
                for (int j = 0; j < share2.Count - i - 1; j++)
                    if (share2[j].num > share2[j + 1].num)
                    {
                        Vertex temp = share2[j];
                        share2[j] = share2[j + 1];
                        share2[j + 1] = temp;
                    }

            foreach (var vertex1 in share1) //Вывод весов рёбер
            {
                foreach (var vertex2 in share2)
                {
                    Edge edge = vertex1.checkEdge(vertex2);
                    if (edge != null)
                        Console.Write("{0,2}", edge.weight + " ");
                    else
                        Console.Write("{0,2}", 0 + " ");

                }
                Console.WriteLine();
            }
        }
        public List<Edge> getEdges(Vertex fst) //Получить список рёбер вершины (служебный)
        {
            List<Edge> lst = new List<Edge>();
            foreach (var vertex in vertices)
            {
                if (fst.checkEdge(vertex) != null) lst.Add(fst.checkEdge(vertex));
            }
            return lst;
        }
        public bool GetGraphShares(ref List<Vertex> share1, ref List<Vertex> share2) //Проверка двудольности
        {
            for(int i = 0;  i < vertices.Count && share1.Count + share2.Count != vertices.Count; i++)
            {
                if (!share1.Contains(vertices[i]) && !share2.Contains(vertices[i]))
                    if (!CheckTS(share1, share2, vertices[i], 1)) return false;
            }
            return true;
        }
        private bool CheckTS(List<Vertex> share1, List<Vertex> share2, Vertex vertex, int share_colour)
        {
            if (!share1.Contains(vertex) && !share2.Contains(vertex)) //Непринадлежащие долям вершины поочерёдно добавлять в доли в соответсвии с цветом, с которым выполнен заход
            {
                if (share_colour == 1)
                {
                    share1.Add(vertex); share_colour = 2;
                }
                else
                {
                    share2.Add(vertex); share_colour = 1;
                }
                foreach (Vertex i in vertices) //Повторять то-же самое для вершин, в которые можно попасть из текущей вершины
                    if (vertex.checkEdge(i) != null)
                        if (!CheckTS(share1, share2, i, share_colour)) return false; //Зайти на следующую вершину с  другим цветом
                return true;
            }
            else //Проверка соотстветсвия цвета, с которым зашли и веришны
            {
                if (share_colour == 1) if (share1.Contains(vertex)) return true;
                if (share_colour == 2) if (share2.Contains(vertex)) return true;
                return false;
            }
        }
        public List<Edge> SearchFstM(List<Vertex> share) //Жадный алгоритм
        {
            List<Vertex> usdVertices = new List<Vertex>();
            List<Edge> matching = new List<Edge>();
            foreach (var vertex in share)
            {
                foreach (var edge in getEdges(vertex))
                    if (!usdVertices.Contains(edge.fst) && !usdVertices.Contains(edge.snd))
                    {
                        usdVertices.Add(edge.fst);
                        usdVertices.Add(edge.snd);
                        matching.Add(edge);
                    }
            }
            return matching;
        }
        public List<Edge> WaveSearchMM(ref List<Vertex> markedVertices) // Поиск максимального паросочетания методом волны
        {
            List<Vertex> share1 = new List<Vertex>();
            List<Vertex> share2 = new List<Vertex>();
            if (!GetGraphShares(ref share1, ref share2)) throw new NonTS("Ошибка: граф не является двудольным..."); //Разбиение графа на две дволи
            List<Edge> matching = SearchFstM(share1); //Поиск начального паросочетания жадным алгоритмом
            bool singleVertex = true; // Наличие непарной вершины

            while ((matching.Count < share1.Count && matching.Count < share2.Count) && singleVertex) 
            {
                List<Vertex> lst = new List<Vertex>();
                markedVertices.Clear();
                List<Edge> chain = new List<Edge>();
                singleVertex = false;
                foreach (var vertex in share1)
                {
                    if (getEdges(vertex).Count > 0) 
                        if (matching.Find(e => e.fst == vertex) == null) //Если найдена непарная вершина
                        {
                            singleVertex = true;
                            markedVertices.Add(vertex);
                            lst.Add(vertex);
                        }
                }
                while (lst.Count > 0)
                {
                    Vertex cVertex = lst[0];
                    lst.RemoveAt(0);
                    if (share1.Contains(cVertex)) // Если непарная вершина находится в первой доле
                    {
                        foreach (var edge in getEdges(cVertex))
                        {
                            if (!matching.Contains(edge) && !markedVertices.Contains(edge.snd)) // Если ребро не входит в паросочетание и ведёт к неотмеченной вершине
                            {
                                // Добавление в цепь и отметка
                                chain.Add(edge);
                                lst.Add(edge.snd);
                                markedVertices.Add(edge.snd);
                            }
                        }
                    }
                    else
                    {
                        singleVertex = true;
                        foreach (var edge in getEdges(cVertex))
                        {
                            if (matching.Find(e => e.fst == edge.snd && e.snd == edge.fst) != null && !markedVertices.Contains(edge.snd)) // Если ребро входит в паросочетание и ведёт к неотмеченной вершине
                            {
                                singleVertex = false;
                                chain.Add(edge); // Добавление в цепь и отметка
                                lst.Add(edge.snd);
                                markedVertices.Add(edge.snd);
                            }
                        }
                        if (singleVertex) // Если останавливаемся на непарной вершине, то чередующаяся цепь найдена
                        {
                            while (cVertex != null) // Увеличение паросочетания вдоль цепи
                            {
                                Edge edge = chain.Find(v => v.snd == cVertex);
                                if (edge != null)
                                {
                                    Edge foundEdge = matching.Find(e => e == edge || e.fst == edge.snd && e.snd == edge.fst);
                                    if (foundEdge != null)
                                        matching.Remove(foundEdge);
                                    else
                                        matching.Add(edge);
                                    cVertex = edge.fst;
                                    chain.Remove(edge);
                                }
                                else
                                    cVertex = null;
                            }
                            break;
                        }
                    }
                }
            }
            return matching;
        }
        public string PrintMatching(List<Edge> matching) // Вывод паросочетания
        {
            string str = "M = { ";
            foreach (Edge edge in matching)
                str += "(" + edge.fst.num + "," + edge.snd.num + ")" + " ";
            str += "}";
            return str;
        }
        public string PrintMaxMin(List<Edge> matching) // Вывод максимального паросочетания минимальной стоимости
        {
            string matching_str = "M = { ";
            string general_cost = "Общая стоимость:  ";
            int cost = 0;
            foreach(Edge edge in matching)
            {
                foreach(Vertex vertex in vertices)
                {
                    foreach(Edge temp_edge in getEdges(vertex))
                    {
                        if(temp_edge.fst.num == edge.fst.num && temp_edge.snd.num == edge.snd.num)
                        {
                            matching_str += "(" + temp_edge.fst.num + "," + temp_edge.snd.num + ")" + " ";
                            general_cost += temp_edge.weight + " + ";
                            cost += temp_edge.weight;
                        }

                    }
                }
            }
            matching_str += "}";
            general_cost = general_cost.Substring(0, general_cost.Length - 2) + "= ";
            return matching_str + "\n" + general_cost + cost.ToString();
        }
        public int CountIzltnVertex(List<Vertex> share1) //Служебный метод для подсчёта изолированных вершин
        {
            int count = 0;
            foreach(Vertex vertex in share1)
            {
                if (getEdges(vertex).Count == 0) count++;
            }
            return count;
        }
        public List<Edge> SearchMaxMin() // Поиск максимального паросочетания минимальной стоимости
        {
            Graph graphClone = new Graph(orientation, vertices); // Вспомогательная копия графа 

            List<Vertex> share1 = new List<Vertex>();
            List<Vertex> share2 = new List<Vertex>();
            if (!graphClone.GetGraphShares(ref share1, ref share2)) throw new NonTS("Ошибка: граф не является двудольным..."); //Разбиение вспомогательного графа на две дволи
            
            foreach(Vertex vertex1 in share1)
            {
                foreach(Vertex vertex2 in share2)
                {
                    if(getEdges(vertex1).Count>0)
                        if (vertex1.checkEdge(vertex2) == null) graphClone.AddEdge(vertex1.num, vertex2.num, int.MaxValue);
                }
            }

            foreach (Vertex vertex1 in share1) // Горизонтальная редукция
            {
                int min = -1;
                Edge edge1, edge2;
                foreach (Vertex v2 in share2) // Поиск минимального веса
                {
                    edge1 = vertex1.checkEdge(v2);
                    if(edge1!=null)
                        if (edge1.weight < min || min == -1)
                            min = edge1.weight;
                }
                foreach (Vertex v2 in share2) // Вычитание минимального веса
                {
                    edge1 = vertex1.checkEdge(v2);
                    edge2 = v2.checkEdge(vertex1);
                    if (edge1 != null && edge2 != null)
                        edge1.weight = edge2.weight -= min;
                }
            }

            List<Vertex> delvertices = new List<Vertex>();
            foreach (Vertex vertex in share2)
            {
                if (getEdges(vertex).Find(e => e.weight == 0) == null) // Проверка каждой вершины на наличие отсутствующего ребра
                    delvertices.Add(vertex);
            }
            
            if (delvertices.Count > 0) // Вертикальная редукция
            {
                foreach (Vertex vertex2 in delvertices)
                {
                    int min = -1;
                    Edge e1, e2;
                    foreach (Vertex vertex1 in share1) // Поиск минимального веса
                    {
                        e2 = vertex2.checkEdge(vertex1);
                        if (e2 != null)
                            if (e2.weight < min || min == -1)
                                min = e2.weight;
                    }
                    foreach (Vertex vertex1 in share1) // Вычитание минимального веса
                    {
                        e2 = vertex2.checkEdge(vertex1);
                        e1 = vertex1.checkEdge(vertex2);
                        if (e1 != null && e2 != null)
                            e2.weight = e1.weight -= min;
                    }
                }
            }


            List<Edge> matching = new List<Edge>();
            while (matching.Count < (share1.Count - CountIzltnVertex(share1)) && matching.Count < share2.Count)
            {
                Graph transformedGraph = new Graph(orientation, vertices); // Составление двудолного графа из нулей в матрице
                Console.WriteLine();
                foreach(Vertex vertex in transformedGraph.vertices)
                {
                    foreach(Edge edge in getEdges(vertex))
                    {
                        transformedGraph.DeleteEdge(edge.fst.num, edge.snd.num);
                    }
                }
                foreach (Vertex v1 in share1)
                {
                    foreach (Vertex v2 in share2)
                    {
                        Edge edge = v1.checkEdge(v2);
                        if (edge != null)
                            if (edge.weight == 0)
                                transformedGraph.AddEdge(v1.num, v2.num, edge.weight);
                    }
                }

                List<Vertex> markedVertices = new List<Vertex>(); // Отмеченные вершины, в ходе построения макс.паросочетания
                matching = transformedGraph.WaveSearchMM(ref markedVertices);

                List<Vertex> markedVerticesV1 = new List<Vertex>();
                List<Vertex> markedVerticesV2 = new List<Vertex>();
                foreach (Vertex vertex in markedVertices)
                {
                    Vertex findedVertexV1 = share1.Find(v => v.num == vertex.num);
                    if (findedVertexV1 != null)
                        markedVerticesV1.Add(findedVertexV1);
                    else
                        markedVerticesV2.Add(share2.Find(v => v.num == vertex.num));
                }

                
                int mmin = -1;
                foreach (Vertex v1 in markedVerticesV1) // Диагональная редукция
                    foreach (Vertex v2 in share2)
                    {
                        if (markedVerticesV2.Contains(v2)) continue;
                        Edge e1 = v1.checkEdge(v2);
                        if (e1 != null)
                            if (e1.weight < mmin || mmin == -1)
                                mmin = e1.weight;
                    }

                foreach (Vertex v1 in markedVerticesV1)
                    foreach (Vertex v2 in share2)
                        if (!markedVerticesV2.Contains(v2))
                        {
                            Edge e1 = v1.checkEdge(v2);
                            Edge e2 = v2.checkEdge(v1);
                            if (e1 != null && e2 != null)
                                e1.weight = e2.weight -= mmin;
                        }

                foreach (Vertex v2 in markedVerticesV2)
                    foreach (Vertex v1 in share1)
                        if (!markedVerticesV1.Contains(v1))
                        {
                            Edge e1 = v1.checkEdge(v2);
                            Edge e2 = v2.checkEdge(v1);
                            if (e1 != null && e2 != null)
                                e1.weight = e2.weight += mmin;
                        }
            }
            return matching;
        }
    }
    class Vertex
    {
        public int num; //Номер вершины
        List<Edge> edges; //Список рёбер

        public Vertex(int num)
        {
            this.num = num;
            edges = new List<Edge>();
        }

        public Edge checkEdge(Vertex snd) //Наличие ребра, ведущего в заданную вершину 
        {
            foreach (Edge edge in edges)
                if (edge.snd.num == snd.num) return edge;
            return null;
        }

        public void addEdge(int weight, Vertex fst, Vertex snd) //Добавление ребра
        {
            if (edges.Find(e => e.snd == snd) != null) throw new NonFindVertex("Ошибка: верщина, в которое должно вести введённое ребро не существует...");
            edges.Add(new Edge(weight, fst, snd));
        }

        public void deleteEdge(Vertex deleted_vertex) //Удаление ребра
        {
            if (checkEdge(deleted_vertex) == null) throw new AttDelInixistingEdge("Ошибка: попытка удалить несуществующее ребро...");
            edges.RemoveAll(edge => edge.snd == deleted_vertex);
        }
        public string printEdge1() //Вывод списка рёбер
        {
            string temp = "";
            foreach (Edge edge in edges)
            {
                temp += Convert.ToString(num) + " " + Convert.ToString(edge.snd.num) + " " + Convert.ToString(edge.weight) + "\n";
            }
            return temp;
        }
        public string printEdge2() //Вывод смежных вершин с (весом)
        {
            string temp = "";
            foreach (Edge edge in edges)
            {
                temp += Convert.ToString(edge.snd.num) + "(" + Convert.ToString(edge.weight) + ")" + "\n";
            }
            return temp;
        }
    }
    class NonTS : Exception
    {
        public NonTS(string msg) : base(msg) { }
    }
    class ErrReadTS : Exception
    {
        public ErrReadTS(string msg) : base(msg) { }
    }
    class ErrReadStrTS : Exception
    {
        public ErrReadStrTS(string msg) : base(msg) { }
    }
    class NonFindVertex : Exception
    {
        public NonFindVertex(string msg) : base(msg) { }
    }
    class AttAddExistingVertex : Exception
    {
        public AttAddExistingVertex(string msg) : base(msg) { }
    }
    class AttAddIncorrectVertex : Exception
    {
        public AttAddIncorrectVertex(string msg) : base(msg) { }
    }
    class AttDelInixistingVertex : Exception
    {
        public AttDelInixistingVertex(string msg) : base(msg) { }
    }
    class AttDelInixistingEdge : Exception
    {
        public AttDelInixistingEdge(string msg) : base(msg) { }
    }
    class AttDelInixistingEdg : Exception
    {
        public AttDelInixistingEdg(string msg) : base(msg) { }
    }
    class Edge
    {
        public int weight; //Вес ребра
        public Vertex fst; //Начало ребра
        public Vertex snd; //Конец ребра

        public Edge(int weight, Vertex fst, Vertex snd)
        {
            this.weight = weight;
            this.fst = fst;
            this.snd = snd;
        }
    }
    class Program
    {
        public static void Main()
        {
            Graph my_graph = null;
            string choice;

            do
            {
                Console.Write("1 - из файла\n2 - вручную\nВыберите способ ввода графа: ");
                choice = Console.ReadLine();
                switch (choice)
                {
                    case "0":
                        break;
                    case "1":
                        Console.Write("\nВ каком виде хранится граф? (1 - матрица двудольного графа, 2 - список рёбер): ");
                        int type = int.Parse(Console.ReadLine());
                        String fullPath = Environment.CurrentDirectory.ToString();
                        fullPath += "\\input.txt";
                        try
                        {
                            my_graph = new Graph(new StreamReader(fullPath), type);
                        }
                        catch(ErrReadTS e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }
                        catch (ErrReadStrTS e)
                        {
                            Console.WriteLine(e.Message);
                            return;
                        }
                        catch
                        {
                            Console.WriteLine("Ошибка при попытке считывания файла...");
                            return;
                        }
                        break;
                    case "2":
                        int num; bool orientation = false;
                        Console.Write("Введите кол-во вершин в графе: ");
                        num = Convert.ToInt32(Console.ReadLine());
                        Console.Write("Является ли граф ориентированным ('1' - да / '0' - нет): ");
                        if (Console.ReadLine() == "1") orientation = true;
                        my_graph = new Graph(num, orientation);
                        break;
                    default:
                        Console.WriteLine("Неопознанный выбор...");
                        break;
                }
            }
            while (choice != "0" && choice != "1" && choice != "2");

            while (choice != "0")
            {
                Console.Write("\n1 - Вывести граф\n2 - Добавить вершину\n3 - Удалить вершину\n4 - Добавить ребро\n5 - Удалить ребро\n6 - Найти max паросочетание\n7 - Найти max паросочетание min стоимости\n0 - Закончить работу с графом\nВыберите действие: ");
                choice = Console.ReadLine();
                switch (choice)
                {
                    case "1":
                        {
                            Console.Write("\n1 - Перечисление множеств\n2 - Матрица смежности\n3 - Списки смежности\n4 - Матрица весов двудольного графа\nВыберите способ вывода: ");
                            int type = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Граф G: ");
                            switch (type)
                            {
                                case 0:
                                    {
                                        Console.WriteLine("Работа с графом закончена");
                                        break;
                                    }
                                case 1:
                                    {
                                        my_graph.Show1();
                                        break;
                                    }
                                case 2:
                                    {
                                        my_graph.Show2();
                                        break;
                                    }
                                case 3:
                                    {
                                        my_graph.Show3();
                                        break;
                                    }
                                case 4:
                                    {
                                        try { my_graph.Show4(); }
                                        catch (NonTS e) { Console.WriteLine(e.Message); break; }
                                        catch { Console.WriteLine("При попытке вывести граф произошла ошибка..."); break; }
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine("Неопознанный выбор...");
                                        break;
                                    }
                            }
                            break;
                        }
                    case "2":
                        {
                            int num;
                            Console.Write("Введите номер вершины: ");
                            num = Convert.ToInt32(Console.ReadLine());
                            try
                            {
                                my_graph.AddVertex(num);
                            }
                            catch (AttAddIncorrectVertex e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch (AttAddExistingVertex e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("При попытке добавить вершину произошла ошибка...");
                                break;
                            }
                            Console.WriteLine("Вершина '" + num + "' успешно добавлена.");
                            break;
                        }
                    case "3":
                        {
                            int num;
                            Console.Write("Введите номер вершины: ");
                            num = Convert.ToInt32(Console.ReadLine());
                            try
                            {
                                my_graph.DeleteVertex(num);
                            }
                            catch (AttDelInixistingVertex e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("При попытке удалить вершину произошла ошибка...");
                                break;
                            }
                            Console.WriteLine("Вершина '" + num + "' успешно удалена из графа.");
                            break;
                        }
                    case "4":
                        {
                            int fst, snd, weight;
                            Console.WriteLine("Введите вершину, из которой выходит ребро: ");
                            fst = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Введите вершину, в которую ведёт ребро: ");
                            snd = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Введите вес ребра: ");
                            weight = Convert.ToInt32(Console.ReadLine());
                            try
                            {
                                my_graph.AddEdge(fst, snd, weight);
                            }
                            catch (NonFindVertex e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("При попытке добавить ребро произошла ошибка...");
                                break;
                            }
                            Console.WriteLine("Ребро " + "(" + fst + "," + snd + ") добавлено в граф");
                            break;
                        }
                    case "5":
                        {
                            int fst, snd;
                            Console.WriteLine("Введите вершину, из которой выходит ребро: ");
                            fst = Convert.ToInt32(Console.ReadLine());
                            Console.WriteLine("Введите вершину, в которую ведёт ребро: ");
                            snd = Convert.ToInt32(Console.ReadLine());
                            try
                            {
                                my_graph.DeleteEdge(fst, snd);
                            }
                            catch (AttDelInixistingEdge e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("При попытке удалить ребро произошла ошибка...");
                                break;
                            }
                            Console.WriteLine("Ребро " + "(" + fst + "," + snd + ") удалено из графа");
                            break;
                        }
                    case "6":
                        {
                            List<Edge> matching;
                            List<Vertex> markedVertex = new List<Vertex>();
                            try
                            {
                                matching = my_graph.WaveSearchMM(ref markedVertex);
                            }
                            catch (NonTS e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.WriteLine("Во время поиска максимального паросочетания произошла ошибка...");
                                break;
                            }
                            Console.WriteLine(my_graph.PrintMatching(matching));
                            break;
                        }
                    case "7":
                        {
                            List<Edge> matching;
                            try
                            {
                                matching = my_graph.SearchMaxMin();
                            }
                            catch (NonTS e)
                            {
                                Console.WriteLine(e.Message);
                                break;
                            }
                            catch
                            {
                                Console.Write("\nВо время поиска максимального паросочетания минимальной стоимости произошла ошибка...");
                                break;
                            }
                            Console.WriteLine(my_graph.PrintMaxMin(matching));
                            break;
                        }
                    default:
                        {
                            Console.WriteLine("Неизвестное действие");
                            break;
                        }
                }   
            }
        }
    }
}