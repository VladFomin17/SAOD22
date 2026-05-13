using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace SAOD22
{
    public partial class SortingForm : Form
    {
        public SortingForm()
        {
            InitializeComponent();
            dataGridView1.RowCount = 5;
            dataGridView1.ColumnCount = 6;
            dataGridView1.Rows[0].Cells[1].Value = "Простое 2ф";
            dataGridView1.Rows[1].Cells[1].Value = "Простое 1ф";
            dataGridView1.Rows[2].Cells[1].Value = "Естественное 2ф";
            dataGridView1.Rows[3].Cells[1].Value = "Естественное 1ф";
            dataGridView1.Rows[4].Cells[1].Value = "Поглощение";

            dataGridView1.Rows[0].Cells[0].Value = true;
            dataGridView1.Rows[1].Cells[0].Value = true;
            dataGridView1.Rows[2].Cells[0].Value = true;
        }

        bool IsSorted(int[] a)
        {
            for (int i = 0; i < a.Length - 1; i++)
                if (a[i] > a[i + 1])
                    return false;
            return true;
        }

        private int[] TwoPhaseMergeSort(int[] a, int seriesLength, ref int comparisons, ref int assignments)
        {
            int seriesCount = (int)Math.Ceiling(a.Length / (double)seriesLength);

            int bLength = 0, cLength = 0, aLength = a.Length;
            while (aLength > 0)
            {
                if (aLength < seriesLength)
                {
                    bLength += aLength;
                    break;
                }

                bLength += seriesLength;
                aLength -= seriesLength * 2;
            }

            cLength = a.Length - bLength;

            int[] b = new int[bLength];
            int[] c = new int[cLength];

            int bIndex = 0, cIndex = 0;
            for (int i = 0; i < seriesCount; i++)
            {
                if (i % 2 == 0)
                {
                    for (int j = 0; j < seriesLength && i * seriesLength + j < a.Length; j++)
                    {
                        b[bIndex++] = a[i * seriesLength + j];
                        assignments++;
                    }
                }
                else
                {
                    for (int j = 0; j < seriesLength && i * seriesLength + j < a.Length; j++)
                    {
                        c[cIndex++] = a[i * seriesLength + j];
                        assignments++;
                    }
                }
            }

            a = TwoInOneMerge(a, b, c, seriesLength, ref comparisons, ref assignments);

            seriesLength *= 2;
            if (seriesLength < a.Length)
                a = TwoPhaseMergeSort(a, seriesLength, ref comparisons, ref assignments);

            return a;
        }

        private int[] TwoInOneMerge(int[] a, int[] b, int[] c, int seriesLength, ref int comparisons, ref int assignments)
        {
            int bSeriesCount = (int)Math.Ceiling(b.Length / (double)seriesLength);
            int cSeriesCount = (int)Math.Ceiling(c.Length / (double)seriesLength);

            int i = 0, j = 0;
            int aIndex = 0;

            for (int k = 0; k < Math.Max(bSeriesCount, cSeriesCount); k++)
            {
                int bEnd = Math.Min(i + seriesLength, b.Length);
                int cEnd = Math.Min(j + seriesLength, c.Length);
                Merge(ref i, ref j, bEnd, cEnd, b, c, a, ref aIndex, ref comparisons, ref assignments);
            }

            return a;
        }

        private void Merge(ref int i, ref int j, int bEnd, int cEnd, int[] b, int[] c, int[] a, ref int aIndex, ref int comparisons, ref int assignments)
        {
            while (i < bEnd && j < cEnd)
            {
                comparisons++;

                if (b[i] < c[j])
                {
                    a[aIndex++] = b[i++];
                    assignments++;
                }
                else
                {
                    a[aIndex++] = c[j++];
                    assignments++;
                }
            }

            while (i < bEnd)
            {
                a[aIndex++] = b[i++];
                assignments++;
            }

            while (j < cEnd)
            {
                a[aIndex++] = c[j++];
                assignments++;
            }
        }

        private int[] OnePhaseMergeSort(int[] a, ref int comparisons, ref int assignments)
        {
            int n = a.Length;

            int[] b = new int[n];
            int[] c = new int[n];
            int[] d = new int[n];
            int[] e = new int[n];

            int bLen, cLen, dLen, eLen;

            int seriesLength = 1;

            SplitToArrays(a, n, seriesLength, b, out bLen, c, out cLen, ref assignments);

            while (seriesLength < n)
            {
                // B + C -> D и E
                MergePassToTwoArrays(
                    b, bLen,
                    c, cLen,
                    d, out dLen,
                    e, out eLen,
                    seriesLength,
                    ref comparisons,
                    ref assignments);

                seriesLength *= 2;

                if (dLen == n)
                {
                    return d;
                }

                // D + E -> B и C
                MergePassToTwoArrays(
                    d, dLen,
                    e, eLen,
                    b, out bLen,
                    c, out cLen,
                    seriesLength,
                    ref comparisons,
                    ref assignments);

                seriesLength *= 2;

                if (bLen == n)
                {
                    return b;
                }
            }

            return a;
        }

        private void SplitToArrays( int[] source, int sourceLength, int seriesLength, int[] b, out int bLen, int[] c, out int cLen, ref int assignments)
        {
            bLen = 0;
            cLen = 0;

            int seriesIndex = 0;

            for (int i = 0; i < sourceLength; i += seriesLength)
            {
                int count = Math.Min(seriesLength, sourceLength - i);

                if (seriesIndex % 2 == 0)
                {
                    for (int j = 0; j < count; j++)
                    {
                        b[bLen++] = source[i + j];
                        assignments++;
                    }
                }
                else
                {
                    for (int j = 0; j < count; j++)
                    {
                        c[cLen++] = source[i + j];
                        assignments++;
                    }
                }

                seriesIndex++;
            }
        }

        private void MergePassToTwoArrays(int[] first, int firstLen, int[] second, int secondLen, int[] result1, out int result1Len, int[] result2, out int result2Len, int seriesLength, ref int comparisons, ref int assignments)
        {
            result1Len = 0;
            result2Len = 0;

            int i = 0;
            int j = 0;
            int seriesIndex = 0;

            while (i < firstLen || j < secondLen)
            {
                int firstEnd = Math.Min(i + seriesLength, firstLen);
                int secondEnd = Math.Min(j + seriesLength, secondLen);

                if (seriesIndex % 2 == 0)
                {
                    MergeToArray(
                        ref i,
                        ref j,
                        firstEnd,
                        secondEnd,
                        first,
                        second,
                        result1,
                        ref result1Len,
                        ref comparisons,
                        ref assignments);
                }
                else
                {
                    MergeToArray(
                        ref i,
                        ref j,
                        firstEnd,
                        secondEnd,
                        first,
                        second,
                        result2,
                        ref result2Len,
                        ref comparisons,
                        ref assignments);
                }

                seriesIndex++;
            }
        }

        private void MergeToArray(ref int i, ref int j, int firstEnd, int secondEnd, int[] first, int[] second, int[] result, ref int resultIndex, ref int comparisons, ref int assignments)
        {
            while (i < firstEnd && j < secondEnd)
            {
                comparisons++;

                if (first[i] < second[j])
                {
                    result[resultIndex++] = first[i++];
                    assignments++;
                }
                else
                {
                    result[resultIndex++] = second[j++];
                    assignments++;
                }
            }

            while (i < firstEnd)
            {
                result[resultIndex++] = first[i++];
                assignments++;
            }

            while (j < secondEnd)
            {
                result[resultIndex++] = second[j++];
                assignments++;
            }
        }

        private int[] NaturalTwoPhaseMergeSort(int[] a, ref int comparisons, ref int assignments)
        {
            int n = a.Length;

            if (n <= 1)
                return a;

            int[] b = new int[n];
            int[] c = new int[n];

            int bLen, cLen;
            int seriesCount;

            while (true)
            {
                // A -> B и C по естественным сериям
                SplitNaturalSeries(
                    a,
                    b,
                    out bLen,
                    c,
                    out cLen,
                    out seriesCount,
                    ref assignments);

                // Если в A осталась одна серия — массив уже отсортирован
                if (seriesCount == 1)
                    return a;

                // B + C -> A
                MergeNaturalSeries(
                    b,
                    bLen,
                    c,
                    cLen,
                    a,
                    ref comparisons,
                    ref assignments);
            }
        }

        private void SplitNaturalSeries(int[] a, int[] b, out int bLen, int[] c, out int cLen, out int seriesCount, ref int assignments)
        {
            bLen = 0;
            cLen = 0;
            seriesCount = 0;

            int i = 0;

            while (i < a.Length)
            {
                bool writeToB = seriesCount % 2 == 0;

                while (true)
                {
                    if (writeToB)
                        b[bLen++] = a[i];
                    else
                        c[cLen++] = a[i];

                    assignments++;

                    i++;

                    if (i >= a.Length)
                        break;

                    if (a[i] < a[i - 1])
                        break;
                }

                seriesCount++;
            }
        }

        private void MergeNaturalSeries(int[] b, int bLen, int[] c, int cLen, int[] a, ref int comparisons, ref int assignments)
        {
            int i = 0;
            int j = 0;
            int aIndex = 0;

            while (i < bLen && j < cLen)
            {
                int bEnd = GetNaturalSeriesEnd(b, i, bLen);
                int cEnd = GetNaturalSeriesEnd(c, j, cLen);

                MergeNaturalRuns(
                    b,
                    ref i,
                    bEnd,
                    c,
                    ref j,
                    cEnd,
                    a,
                    ref aIndex,
                    ref comparisons,
                    ref assignments);
            }

            while (i < bLen)
            {
                a[aIndex++] = b[i++];
                assignments++;
            }

            while (j < cLen)
            {
                a[aIndex++] = c[j++];
                assignments++;
            }
        }

        private int GetNaturalSeriesEnd(int[] array, int start, int length)
        {
            int i = start + 1;

            while (i < length && array[i] >= array[i - 1])
                i++;

            return i;
        }

        private void MergeNaturalRuns(int[] b, ref int i, int bEnd, int[] c, ref int j, int cEnd, int[] a, ref int aIndex, ref int comparisons, ref int assignments)
        {
            while (i < bEnd && j < cEnd)
            {
                comparisons++;

                if (b[i] <= c[j])
                {
                    a[aIndex++] = b[i++];
                    assignments++;
                }
                else
                {
                    a[aIndex++] = c[j++];
                    assignments++;
                }
            }

            while (i < bEnd)
            {
                a[aIndex++] = b[i++];
                assignments++;
            }

            while (j < cEnd)
            {
                a[aIndex++] = c[j++];
                assignments++;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void onSortClick(object sender, EventArgs e)
        {
            int n = (int)arraySize.Value;
            Random rnd = new Random();

            int[] source = new int[n];
            for (int i = 0; i < n; i++)
                source[i] = rnd.Next(n);

            int comparisons, assignments;

            if (Convert.ToBoolean(dataGridView1.Rows[0].Cells[0].Value))
            {
                int[] sortingArray = (int[])source.Clone();
                comparisons = 0;
                assignments = 0;

                int t1 = Environment.TickCount;
                sortingArray = TwoPhaseMergeSort(sortingArray, 1, ref comparisons, ref assignments);
                int time = Environment.TickCount - t1;

                dataGridView1.Rows[0].Cells[2].Value = comparisons;
                dataGridView1.Rows[0].Cells[3].Value = assignments;
                dataGridView1.Rows[0].Cells[4].Value = time;
                dataGridView1.Rows[0].Cells[5].Value = IsSorted(sortingArray) ? "Да" : "Нет";
            }
            else
            {
                dataGridView1.Rows[0].Cells[2].Value = "";
                dataGridView1.Rows[0].Cells[3].Value = "";
                dataGridView1.Rows[0].Cells[4].Value = "";
                dataGridView1.Rows[0].Cells[5].Value = "";
            }

            if (Convert.ToBoolean(dataGridView1.Rows[1].Cells[0].Value))
            {
                int[] sortingArray = (int[])source.Clone();
                comparisons = 0;
                assignments = 0;

                int t1 = Environment.TickCount;
                sortingArray = OnePhaseMergeSort(sortingArray, ref comparisons, ref assignments);
                int time = Environment.TickCount - t1;

                dataGridView1.Rows[1].Cells[2].Value = comparisons;
                dataGridView1.Rows[1].Cells[3].Value = assignments;
                dataGridView1.Rows[1].Cells[4].Value = time;
                dataGridView1.Rows[1].Cells[5].Value = IsSorted(sortingArray) ? "Да" : "Нет";
            }
            else
            {
                dataGridView1.Rows[1].Cells[2].Value = "";
                dataGridView1.Rows[1].Cells[3].Value = "";
                dataGridView1.Rows[1].Cells[4].Value = "";
                dataGridView1.Rows[1].Cells[5].Value = "";
            }

            if (Convert.ToBoolean(dataGridView1.Rows[2].Cells[0].Value))
            {
                int[] sortingArray = (int[])source.Clone();
                comparisons = 0;
                assignments = 0;

                int t1 = Environment.TickCount;
                sortingArray = NaturalTwoPhaseMergeSort(sortingArray, ref comparisons, ref assignments);
                int time = Environment.TickCount - t1;

                dataGridView1.Rows[2].Cells[2].Value = comparisons;
                dataGridView1.Rows[2].Cells[3].Value = assignments;
                dataGridView1.Rows[2].Cells[4].Value = time;
                dataGridView1.Rows[2].Cells[5].Value = IsSorted(sortingArray) ? "Да" : "Нет";
            }
            else
            {
                dataGridView1.Rows[2].Cells[2].Value = "";
                dataGridView1.Rows[2].Cells[3].Value = "";
                dataGridView1.Rows[2].Cells[4].Value = "";
                dataGridView1.Rows[2].Cells[5].Value = "";
            }
        }
    }
}
