# Работа с графами.

Ввод графа осуществляется из файла, в первой строке которого находится число вершин графа и через пробел 0 или 1, показывающие, является ли граф ориентированным.
Последующие строки содержат описания рёбер графа – три числа: начальная вершина, конечная вершина, вес ребра.
Пример файла, который описывает граф, приведённый ниже:

4 1 </br>
1 2 5 </br>
1 4 6 </br>
2 4 2 </br>
4 3 3 </br>
3 1 5 </br>

2.	Вывод осуществляется в трёх видах:</br>
a.	Перечисление множеств V и E графа. Множество E в первой строке, множество V – построчно, по аналогии с файлом ввода.
b.	В виде матрицы смежности / матрицы весов. </br>
c.	В виде списков смежности (в каждой строке описан список смежности одной вершины, первое число в строке – номер вершины, дальше идут номера вершин, смежных данной, вес ребра указать в скобках) </br>
3.	Реализованны классы: </br>
    a.	Ребро: edge </br>
        i.	Вес ребра </br>
        ii.	Конец ребра </br>
        iii. Конструктор(ы) </br>
    b.	Вершина: vertex </br>
        i.	Номер вершины </br>
        ii.	Список рёбер </br>
        iii.	Конструктор(ы) </br>
        iv.	Добавление ребра </br>
    c.	Граф: graph </br>
        i.	Список вершин </br>
        ii.	Конструктор без параметров </br>
        iii.	Конструктор, принимающий кол-во вершин и ориентированность графа
        iv.	Конструктор, принимающий поток ввода </br>
        v.	Добавление вершины </br>
        vi.	Добавление ребра </br>
        vii.	Удаление вершины </br>
        viii.	Удаление ребра </br>
        ix.	Методы для каждого вида вывода </br>
4. Реализован алгоритм поиска максимального паросочетания. </br>
5. Реализован алгоритм поиска максимального потока в сети.