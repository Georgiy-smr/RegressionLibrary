namespace Regression.MathService
{
    /// <summary>
    /// Работа с коэффициентам по методу Гаусса
    /// </summary>
    internal class Gaus
    {
        public double[] Roots(double[,] X, double[,] Y)
        {
            bool isnan = true;
            int CountPoints = Y.GetUpperBound(0) + 1;
            double[] RootGausMethod = new double[CountPoints];
            double[,] MatrixForGaus = new double[CountPoints, CountPoints + 1];
            MatrixForGaus = UnificationGausMatrix(X, Y, MatrixForGaus);
            RootGausMethod = Gauss(MatrixForGaus, ref isnan);
            while (!isnan)
            {
                RowsPlusRows(MatrixForGaus, (int)RootGausMethod[0], (int)RootGausMethod[1]);
                RootGausMethod = Gauss(MatrixForGaus, ref isnan);
            }
            double[] Array = new double[Y.GetUpperBound(0) + 1];
            for (int i = 0; i < RootGausMethod.GetUpperBound(0) + 1; i++)
            {
                Array[i] = RootGausMethod[i];
            }
            return Array;
        }
        public static void RowsPlusRows(double[,] Gaus, int Row, int plusRows)
        {

            for (var j = 0; j < Gaus.GetLength(1); j++)
            {
                Gaus[Row, j] += Gaus[plusRows, j];
            }
        }
        public static double[,] UnificationGausMatrix(double[,] X, double[,] Y, double[,] Gaus)
        {
            for (int i = 0; i < X.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < X.GetUpperBound(1) + 1; j++)
                {
                    Gaus[i, j] = X[i, j];
                }
            }
            for (int i = 0; i < Y.GetUpperBound(0) + 1; i++)
            {
                for (int j = 0; j < Y.GetUpperBound(1) + 1; j++)
                {
                    Gaus[i, X.GetUpperBound(1) + 1] = Y[i, j];
                }
            }
            return Gaus;
        }
        public static double[] Gauss(double[,] Matrix, ref bool ISNAN)
        {
            //MatrixToString(Matrix);
            int n = Matrix.GetLength(0); //Размерность начальной матрицы (строки)
            double[,] Matrix_Clone = new double[n, n + 1]; //Матрица-дублер
            double[,] Matrix_Clone2 = new double[n, n + 1]; //Матрица-дублер
            double[] Answer = new double[n]; //матрица ответов
            ISNAN = true;// флаг деления на ноль
            for (int i = 0; i < n; i++)
                for (int j = 0; j < n + 1; j++)
                {
                    Matrix_Clone[i, j] = Matrix[i, j];
                    Matrix_Clone2[i, j] = Matrix[i, j];
                }
            // Прямой ход (Зануление нижнего левого угла)
            for (int k = 0; k < n; k++) //k-номер строки
            {
                for (int i = 0; i < n + 1; i++) //i-номер столбца
                {
                    Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k]; //Деление k-строки на первый член !=0 для преобразования его в единицу

                    if (double.IsNaN(Matrix_Clone[k, i]))//check isnan 
                    {
                        ISNAN = false;
                        Answer[0] = k; //index is nan value
                        Answer[1] = FindRowInNoZeroElement(Matrix_Clone2, k); //index is nonzero value
                        return Answer;
                    }
                }


                for (int i = k + 1; i < n; i++) //i-номер следующей строки после k
                {
                    double K = Matrix_Clone[i, k] / Matrix_Clone[k, k]; //Коэффициент
                    for (int j = 0; j < n + 1; j++) //j-номер столбца следующей строки после k
                        Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K; //Зануление элементов матрицы ниже первого члена, преобразованного в единицу
                }
                for (int i = 0; i < n; i++) //Обновление, внесение изменений в начальную матрицу
                    for (int j = 0; j < n + 1; j++)
                        Matrix[i, j] = Matrix_Clone[i, j];

            }
            // Обратный ход (Зануление верхнего правого угла)
            for (int k = n - 1; k > -1; k--) //k-номер строки
            {
                for (int i = n; i > -1; i--) //i-номер столбца
                    Matrix_Clone[k, i] = Matrix_Clone[k, i] / Matrix[k, k];
                for (int i = k - 1; i > -1; i--) //i-номер следующей строки после k
                {
                    double K = Matrix_Clone[i, k] / Matrix_Clone[k, k];
                    for (int j = n; j > -1; j--) //j-номер столбца следующей строки после k
                        Matrix_Clone[i, j] = Matrix_Clone[i, j] - Matrix_Clone[k, j] * K;
                }
            }

            // Отделяем от общей матрицы ответы

            for (int i = 0; i < n; i++)
                Answer[i] = Matrix_Clone[i, n];

            return Answer;
        }

        /// <summary>
        /// Находим стоку содержащую не нулевое значение
        /// </summary>
        /// <param name="Gaus"></param>
        /// <param name="StartRow"></param>
        /// <returns></returns>
        public static int FindRowInNoZeroElement(double[,] Gaus, int StartRow)
        {
            int maxrow = 0;
            double Value = 0;
            for (int i = StartRow + 1; i < Gaus.GetUpperBound(0) + 1; i++)
            {

                //if (Math.Abs(Gaus[i, StartRow]) > Value)
                if (Math.Abs(Gaus[i, StartRow]) != Value)
                {
                    // Value = Math.Abs(Gaus[i, StartRow]);
                    maxrow = i;
                    //break; //Лучше находить последний ненулевой эелемент. ИМХО алгорим быстрей проходит)
                }
            }
            return maxrow;
        }


    }
}

