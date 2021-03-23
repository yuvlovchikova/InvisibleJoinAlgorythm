using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InvisibleJoinAlgorythm
{
    class Program
    {
        static void Main(string[] args)
        {
            string folderForTables = args[0];
            string pathInput = args[1];
            string pathOutput = args[2];
            string[] inputData = File.ReadAllLines(pathInput);
            string outputFilters = inputData[0];
            int numberOfFilters = int.Parse(inputData[1]);
            if (numberOfFilters == 0)
            {
                File.WriteAllText(pathOutput,"");
            }
            else
            {
                List<List<string>> listOfFilters = new List<List<string>>();
                //парсинг фильтров
                for (int i = 2; i < inputData.Length; i++)
                {
                    string[] parametrs = inputData[i].Split(' ');
                    List<string> parametrsOfFilter = new List<string>();
                    string[] names = parametrs[0].Split('.');
                    parametrsOfFilter.Add(names[0]);
                    parametrsOfFilter.Add(names[1]);
                    parametrsOfFilter.Add(parametrs[1]);
                    string thirdParametr = string.Join(" ", parametrs.Skip(2));

                    string[] thirdParametrSplited = thirdParametr.Split('\'');
                    if (thirdParametrSplited[0] == "")
                        parametrsOfFilter.Add(thirdParametrSplited[1]);
                    else
                        parametrsOfFilter.Add(thirdParametrSplited[0]);
                    listOfFilters.Add(parametrsOfFilter);
                }

                string[] FactResellerSales = new string[] { "ProductKey", "OrderDateKey", "ResellerKey", "EmployeeKey", "PromotionKey", "CurrencyKey", "SalesTerritoryKey", "SalesOrderNumber", "SalesOrderLineNumber", "OrderQuantity", "CarrierTrackingNumber", "CustomerPONumber" };

                string[] DimProduct = new string[] { "ProductKey", "ProductAlternateKey", "EnglishProductName", "Color", "SafetyStockLevel", "ReorderPoint", "SizeRange", "DaysToManufacture", "StartDate" };

                string[] DimReseller = new string[] { "ResellerKey", "ResellerAlternateKey", "Phone", "BusinessType", "ResellerName", "NumberEmployees", "OrderFrequency", "ProductLine", "AddressLine1", "BankName", "YearOpened" };

                string[] DimCurrency = new string[] { "CurrencyKey", "CurrencyAlternateKey", "CurrencyName" };

                string[] DimPromotion = new string[] { "PromotionKey", "PromotionAlternateKey", "EnglishPromotionName", "EnglishPromotionType", "EnglishPromotionCategory", "StartDate", "EndDate", "MinQty" };

                string[] DimSalesTerritory = new string[] { "SalesTerritoryKey", "SalesTerritoryAlternateKey", "SalesTerritoryRegion", "SalesTerritoryCountry", "SalesTerritoryGroup" };

                string[] DimEmployee = new string[] { "EmployeeKey", "FirstName", "LastName", "Title", "BirthDate", "LoginID", "EmailAddress", "Phone", "MaritalStatus", "Gender", "PayFrequency", "VacationHours", "SickLeaveHours", "DepartmentName", "StartDate" };

                string[] DimDate = new string[] { "DateKey", "FullDateAlternateKey", "DayNumberOfWeek", "EnglishDayNameOfWeek", "DayNumberOfMonth", "DayNumberOfYear", "WeekNumberOfYear", "EnglishMonthName", "MonthNumberOfYear", "CalendarQuarter", "CalendarYear", "CalendarSemester", "FiscalQuarter", "FiscalYear", "FiscalSemester" };

                string[][] tables = new string[][] { FactResellerSales, DimProduct, DimReseller, DimCurrency, DimPromotion, DimSalesTerritory, DimEmployee, DimDate };
                string[] namesOfTables = new string[] { "FactResellerSales", "DimProduct", "DimReseller", "DimCurrency", "DimPromotion", "DimSalesTerritory", "DimEmployee", "DimDate" };

                //фаза 1
                List<RoaringBitmap> bitmaps = new List<RoaringBitmap>();
                //проходим по каждому фильтру
                for (int i = 0; i < listOfFilters.Count(); i++)
                {
                    RoaringBitmap roaringBitmap = new RoaringBitmap();
                    bitmaps.Add(roaringBitmap);
                    string nameOfTable = listOfFilters[i][0];
                    string nameOfColumn = listOfFilters[i][1];
                    string predicate = listOfFilters[i][2];
                    string reference = listOfFilters[i][3];
                    string path = folderForTables;
                    if (nameOfTable == "FactResellerSales")
                        path += $"/{nameOfTable}.{nameOfColumn}.csv";
                    else path += $"/{nameOfTable}.csv";
                    //считываем данные из таблицы, соответствующей фильтру
                    string[] tablesData = File.ReadAllLines(path);
                    List<List<string>> listTablesData = new List<List<string>>();
                    for (int j = 0; j < tablesData.Length; j++)
                    {
                        listTablesData.Add(new List<string>());
                        string[] tableRow = tablesData[j].Split('|');
                        for (int k = 0; k < tableRow.Length; k++)
                        {
                            listTablesData[j].Add(tableRow[k]);
                        }
                    }
                    int tableIndex = Array.IndexOf(namesOfTables, nameOfTable);
                    int columnIndex;
                    if (nameOfTable == "FactResellerSales")
                        columnIndex = 0;
                    else columnIndex = Array.IndexOf(tables[tableIndex], nameOfColumn);
                    for (int j = 0; j < listTablesData.Count(); j++)
                    {
                        string verifiable = listTablesData[j][columnIndex];
                        bool value = true;
                        //проверяем выполнение условия
                        if (predicate == "=")
                            value = (verifiable.Equals(reference));
                        else
                        {
                            if (predicate == "<>")
                                value = (!(verifiable.Equals(reference)));
                            else
                            {
                                int verifiableInt = int.Parse(verifiable);
                                int referenceInt = int.Parse(reference);
                                if (predicate == "<")
                                    value = (verifiableInt < referenceInt);
                                if (predicate == ">")
                                    value = (verifiableInt > referenceInt);
                                if (predicate == "<=")
                                    value = (verifiableInt <= referenceInt);
                                if (predicate == ">=")
                                    value = (verifiableInt >= referenceInt);
                            }
                        }
                        //чтобы не тратить лишние операции, добавляем в битмап элемент только в случае выполнения условия
                        if (value)
                        {
                            bitmaps[i].Set(nameOfTable == "FactResellerSales" ? j : int.Parse(listTablesData[j][0]), true);
                        }
                    }
                }

                //фаза 2
                string factTablePath = folderForTables + $"/FactResellerSales.";
                List<List<string>> listFactTableData = new List<List<string>>();
                for (int j = 0; j < FactResellerSales.Length; j++)
                {
                    listFactTableData.Add(new List<string>());
                    string[] columnOfFactTableData = File.ReadAllLines(factTablePath + $"{FactResellerSales[j]}.csv");
                    for (int k = 0; k < columnOfFactTableData.Length; k++)
                    {
                        listFactTableData[j].Add(columnOfFactTableData[k]);
                    }
                }
                List<RoaringBitmap> factTableBitmaps = new List<RoaringBitmap>();
                for (int i = 0; i < bitmaps.Count(); i++)
                {
                    factTableBitmaps.Add(new RoaringBitmap());
                    string nameOfFilterTable = listOfFilters[i][0];
                    int tableIndex = Array.IndexOf(namesOfTables, nameOfFilterTable);
                    int indexOfColumnInFactTable = Array.IndexOf(FactResellerSales, tables[tableIndex][0]);
                    string columnNameOfFactTable = tables[tableIndex][0];
                    if (columnNameOfFactTable == "DateKey")
                        columnNameOfFactTable = "OrderDateKey";
                    string[] columnOfFactTableData = File.ReadAllLines(folderForTables + $"/FactResellerSales.{columnNameOfFactTable}.csv");
                    for (int j = 0; j < columnOfFactTableData.Count(); j++)
                    {
                        if (bitmaps[i].Get(nameOfFilterTable == "FactResellerSales" ? j : int.Parse(columnOfFactTableData[j])))
                            factTableBitmaps[i].Set(j, true);
                    }
                }

                RoaringBitmap resultBitmap = new RoaringBitmap();
                resultBitmap = factTableBitmaps[0];
                for (int i = 1; i < factTableBitmaps.Count(); i++)
                {
                    resultBitmap.And(factTableBitmaps[i]);
                }

                //вывод
                List<List<string>> outputData = new List<List<string>>();
                string[] outputFiltersArray = outputFilters.Split(',');
                for (int i = 0; i < outputFiltersArray.Length; i++)
                {
                    string path = folderForTables + $"/{outputFiltersArray[i]}.csv";
                    string[] tableData = File.ReadAllLines(path);
                    List<string> list = new List<string>();
                    for (int k = 0; k < tableData.Length; k++)
                    {
                        if (resultBitmap.Get(k))
                            list.Add(tableData[k]);
                    }
                    outputData.Add(list);
                }
                List<string> resultList = new List<string>();
                for (int i = 0; i < outputData[0].Count; i++)
                {
                    List<string> something = new List<string>();
                    for (int j = 0; j < outputData.Count; j++)
                    {
                        something.Add(outputData[j][i]);
                    }
                    resultList.Add(string.Join("|", something));

                }
                File.WriteAllText(pathOutput, string.Join("\n", resultList));
            }
        }
    }

    public abstract class Bitmap
    {
        private int cardinality;
        public int Cardinality {
            get => cardinality;
            set {
                cardinality = value;
            }
        }

        private int[] container;
        public int[] Container {
            get => container;
            set {
                container = value;
            }
        }

        public int Size {
            get {
                if (container == null) return 0;
                return container.Length;
            }
        }

        public abstract void And(Bitmap other);

        public abstract void Set(int i, bool value);

        public abstract bool Get(int i);

        //возвращает количество единиц в двоичном представлении числа
        public static int GetHammingWeight(int w)
        {
            int bitCount = 0;
            while (w > 0)
            {
                //из w вычитаем такое же число, но у которого нулевой бит равен нулю.
                int bit = w - ((w >> 1) * 2);
                bitCount += bit;
                w >>= 1;
            }
            return bitCount;
        }

        //данное натурльное число представляет как массив целых чисел, равных номерам разрядов, на месте которых в двоичном представлении числа стоят 1
        public static int[] ConvertIntegerIntoArray(int w)
        {
            int[] arr = new int[0];
            int t;
            int i = 0;
            while (w > 0)
            {
                Array.Resize(ref arr, arr.Length + 1);
                //w&(-w) возвращает 2 в степени номер наименьшего единичного бита, начиная с нулевого. x-1 возвращает int, в двоичной записи
                //который получается из х заменой всех правых нулей единицами, а самую правую единицу - нулём. Таким образом, (w & (-w)) - 1
                //равно числу, в двоичной записи которое состоит из одних единиц, количество которых равно номеру наименьшего единичного бита числа w.
                t = (w & (-w)) - 1;
                arr[i] = GetHammingWeight(t);
                i++;
                w = w & (w - 1);
            }
            return arr;
        }
    }

    public class ArrayContainer : Bitmap
    {
        public ArrayContainer()
        {
            Container = new int[0];
            Cardinality = 0;
        }
        public override void And(Bitmap other)
        {
            /*Оптимизированная альтернативная реализация метода конъюнкции:
             * 
            if (other is ArrayContainer)
            {
                for (int i = 0; i < other.Size; i++)
                    Set(other.Container[i], other.Get(i)&&Get(i));
            }

            if (other is BitmapContainer)
            {
                for (int i = 0; i < other.Size; i++)
                {
                    int w = other.Container[i];
                    int t = 0;
                    while (w > 0)
                    {
                        //w&(-w) возвращает 2 в степени номер наименьшего единичного бита, начиная с нулевого. x-1 возвращает int, в двоичной записи
                        //который получается из х заменой всех правых нулей единицами, а самую правую единицу - нулём. Таким образом, (w & (-w)) - 1
                        //равно числу, в двоичной записи которое состоит из одних единиц, количество которых равно номеру наименьшего единичного бита числа w.
                        t = (w & (-w)) - 1;
                        Set(GetHammingWeight(t), true);
                        w = w & (w - 1);
                    }
                }
            
            }
            */

            for (int i = 0; i < Size;)
            {
                var val = Container[i];
                if (other.Get(val))
                    i++;
                else
                    Set(val, false);
            }
        }

        //метод бинарного поиска номера индекса в отсортированном массиве, куда нужно поставить передаваемое число, чтобы массив остался упорядоченным (с учетом сдвига
        //всех чисел, больших, чем передаваеммое, на один индекс вправо)
        public static int FindIndex(int[] arr, int x)
        {
            int left = 0;
            int right = arr.Length - 1;
            while (left != right)
            {
                //если длина отрезка [left;right] нечётна, то x сравнивается со средним элементом; 
                //если чётна - то сближайшим слева к середине элементом.
                if (x <= arr[(left + right) / 2])
                    right = (left + right) / 2;
                else
                    left = ((left + right) / 2) + 1;
            }
            return left;
        }

        public override bool Get(int x)
        {
            int index = 0;
            if (Size == 0)
                return false;
            index = FindIndex(Container, x);
            return (Container[index] == x);
        }

        public override void Set(int x, bool value)
        {
            int[] arr = Container;
            int index = 0;
            if (arr.Length != 0)
                index = FindIndex(arr, x);
            if (value)
            {
                if (arr.Length == 0)
                {
                    Array.Resize(ref arr, 1);
                    arr[0] = x;
                    Cardinality++;
                }
                else
                {
                    if (arr[index] != x)
                    {
                        if (index == arr.Length - 1)
                            index++;
                        Array.Resize(ref arr, arr.Length + 1);
                        if (arr.Length == 2)
                        {
                            if (x > arr[0])
                                arr[1] = x;
                            else
                            {
                                arr[1] = arr[0];
                                arr[0] = x;
                            }
                        }
                        else
                        {
                            if (index == arr.Length - 1)
                            {
                                arr[index] = x;
                            }
                            else
                            {
                                for (int i = arr.Length - 2; i >= index; i--)
                                    arr[i + 1] = arr[i];
                                arr[index] = x;
                            }
                        }
                        Cardinality++;
                    }
                }
            }
            else
            {
                if (arr.Length > 0 && arr[index] == x)
                {
                    Array.Clear(arr, index, 1);
                    if (arr.Length > 1)
                    {
                        for (int i = index; i < arr.Length - 1; i++)
                            arr[i] = arr[i + 1];
                    }
                    Array.Resize(ref arr, arr.Length - 1);
                    Cardinality--;
                }
            }
            Container = arr;
        }

        public static explicit operator BitmapContainer(ArrayContainer arrayContainer)
        {
            BitmapContainer bitmapContainer = new BitmapContainer();
            for (int i = 0; i < arrayContainer.Size; i++)
                bitmapContainer.Set(arrayContainer.Container[i], true);
            return bitmapContainer;
        }
    }

    public class BitmapContainer : Bitmap
    {
        public const int IntSize = 32;

        public BitmapContainer()
        {
            Container = new int[1 << 11];
            Cardinality = 0;
        }

        public override void And(Bitmap other)
        {
            if (other is BitmapContainer)
            {
                for (int i = 0; i < Size * IntSize; i++)
                {
                    if (Get(i))
                        Set(i, other.Get(i));
                }
            }
            if (other is ArrayContainer)
            {
                for (int i = 0; i < other.Size; i++)
                {
                    int x = other.Container[i];
                    if (Get(x))
                        Set(x, other.Get(x));
                }
            }
        }

        public override bool Get(int i)
        {
            int chunkIndex = i / IntSize;
            int numIndex = i % IntSize;
            int x = 1 << numIndex;
            return (((Container[chunkIndex] & x)) != 0);
        }

        public override void Set(int i, bool value)
        {
            int chunkIndex = i / IntSize;
            int numIndex = i % IntSize;
            int x = 1 << numIndex;
            if (value)
            {
                //если нужно добавить 1 на i-тое место, то выполняем дизъюнкцию исходного числа с числом из одних нулей, кроме i-того разряда,
                //на котором стоит единица
                Container[chunkIndex] |= x;
                Cardinality++;
            }
            else
            {
                //если нужно удалить 1 с i-того места, то выполняем конъюнкцию исходного числа с числом из одних единиц, кроме i-того разряда,
                //на котором стоит нуль
                Container[chunkIndex] &= (~x);
                Cardinality--;
            }
        }

        public static explicit operator ArrayContainer(BitmapContainer bitmapContainer)
        {
            ArrayContainer arrayContainer = new ArrayContainer();
            for (int i = 0; i < bitmapContainer.Size * IntSize; i++)
            {
                //int[] arrCurrent = ConvertIntegerIntoArray(bitmapContainer.Container[i]);
                if (bitmapContainer.Get(i))
                    arrayContainer.Set(i, true);
            }
            return arrayContainer;
        }
    }

    public class RoaringBitmap : Bitmap
    {
        public const int ContainerSize = 1 << 16;
        public const int BoandaryCardinality = 1 << 12;

        private Bitmap[] bitmapArray = new Bitmap[0];
        public Bitmap[] BitmapArray {
            get => bitmapArray;
            set {
                bitmapArray = value;
            }
        }

        public override void And(Bitmap other)
        {
            RoaringBitmap otherRoaring = (RoaringBitmap)other;

            for (int i = 0; i < BitmapArray.Length; i++)
            {
                Bitmap thisContainer = BitmapArray[i];
                if (i >= otherRoaring.BitmapArray.Length)
                {
                    bitmapArray[i] = null;
                    continue;
                }
                Bitmap otherContainer = otherRoaring.BitmapArray[i];
                if (otherContainer == null)
                {
                    BitmapArray[i] = null;
                    continue;
                }
                if (thisContainer != null && otherContainer != null)
                {
                    thisContainer.And(otherContainer);
                    if (thisContainer.Cardinality < BoandaryCardinality && (thisContainer is BitmapContainer))
                    {
                        ArrayContainer thisArrayContainer = (ArrayContainer)((BitmapContainer)thisContainer);
                        BitmapArray[i] = thisArrayContainer;
                    }
                }
            }
        }

        public override bool Get(int i)
        {
            int containerIndex = i / ContainerSize;
            int numberIndex = i % ContainerSize;
            return containerIndex < bitmapArray.Length && BitmapArray[containerIndex] != null && bitmapArray[containerIndex].Get(numberIndex);
        }

        public override void Set(int i, bool value)
        {
            int containerIndex = i / ContainerSize;
            int numberIndex = i % ContainerSize;
            if (containerIndex >= bitmapArray.Length)
                if (value)
                    Array.Resize(ref bitmapArray, containerIndex + 1);
                else return;
            if (bitmapArray[containerIndex] == null)
                bitmapArray[containerIndex] = new ArrayContainer();
            Bitmap container = bitmapArray[containerIndex];
            container.Set(numberIndex, value);
            if (container.Cardinality < BoandaryCardinality && (container is BitmapContainer))
                bitmapArray[containerIndex] = (ArrayContainer)(container as BitmapContainer);
            if (container.Cardinality >= BoandaryCardinality && (container is ArrayContainer))
                bitmapArray[containerIndex] = (BitmapContainer)(container as ArrayContainer);
        }
    }
}