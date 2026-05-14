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
            dataGridView1.Rows[3].Cells[0].Value = true;
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
                SplitNaturalSeries(
                    a,
                    b,
                    out bLen,
                    c,
                    out cLen,
                    out seriesCount,
                    ref assignments);

                if (seriesCount == 1)
                    return a;

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

        private int[] OnePhaseNaturalMergeSort(int[] a, ref int comparisons, ref int assignments)
        {
            int[] b = new int[a.Length];
            int[] c = new int[a.Length];

            int bLength = 0, cLength = 0;
            int aIndex = 0;
            int toB = 0;

            while (aIndex < a.Length)
            {
                int start = aIndex;

                while (aIndex + 1 < a.Length && a[aIndex] <= a[aIndex + 1])
                {
                    comparisons++;
                    aIndex++;
                }

                if (aIndex + 1 < a.Length)
                    comparisons++;

                if (toB % 2 == 0)
                {
                    for (int j = start; j <= aIndex; j++)
                    {
                        b[bLength++] = a[j];
                        assignments++;
                    }
                }
                else
                {
                    for (int j = start; j <= aIndex; j++)
                    {
                        c[cLength++] = a[j];
                        assignments++;
                    }
                }

                aIndex++;
                toB++;
            }

            if (cLength == 0)
                return a;

            int[] d = new int[a.Length];
            int[] e = new int[a.Length];

            int dLength = 0, eLength = 0;
            int series = 0;

            while (true)
            {
                if (series % 2 == 0)
                {
                    int bInd = 0, cInd = 0;
                    int deFlag = 0;

                    dLength = 0;
                    eLength = 0;

                    while (bInd < bLength && cInd < cLength)
                    {
                        int bSeriesStart = bInd;
                        int bSeriesEnd = bSeriesStart;

                        while (bSeriesEnd + 1 < bLength && b[bSeriesEnd] <= b[bSeriesEnd + 1])
                        {
                            comparisons++;
                            bSeriesEnd++;
                        }

                        if (bSeriesEnd + 1 < bLength)
                            comparisons++;

                        int cSeriesStart = cInd;
                        int cSeriesEnd = cSeriesStart;

                        while (cSeriesEnd + 1 < cLength && c[cSeriesEnd] <= c[cSeriesEnd + 1])
                        {
                            comparisons++;
                            cSeriesEnd++;
                        }

                        if (cSeriesEnd + 1 < cLength)
                            comparisons++;

                        int i = bSeriesStart;
                        int j = cSeriesStart;

                        if (deFlag % 2 == 0)
                        {
                            while (i <= bSeriesEnd && j <= cSeriesEnd)
                            {
                                comparisons++;

                                if (b[i] <= c[j])
                                    d[dLength++] = b[i++];
                                else
                                    d[dLength++] = c[j++];

                                assignments++;
                            }

                            while (i <= bSeriesEnd)
                            {
                                d[dLength++] = b[i++];
                                assignments++;
                            }

                            while (j <= cSeriesEnd)
                            {
                                d[dLength++] = c[j++];
                                assignments++;
                            }
                        }
                        else
                        {
                            while (i <= bSeriesEnd && j <= cSeriesEnd)
                            {
                                comparisons++;

                                if (b[i] <= c[j])
                                    e[eLength++] = b[i++];
                                else
                                    e[eLength++] = c[j++];

                                assignments++;
                            }

                            while (i <= bSeriesEnd)
                            {
                                e[eLength++] = b[i++];
                                assignments++;
                            }

                            while (j <= cSeriesEnd)
                            {
                                e[eLength++] = c[j++];
                                assignments++;
                            }
                        }

                        bInd = bSeriesEnd + 1;
                        cInd = cSeriesEnd + 1;
                        deFlag++;
                    }

                    while (bInd < bLength)
                    {
                        if (deFlag % 2 == 0)
                            d[dLength++] = b[bInd++];
                        else
                            e[eLength++] = b[bInd++];

                        assignments++;
                    }

                    while (cInd < cLength)
                    {
                        if (deFlag % 2 == 0)
                            d[dLength++] = c[cInd++];
                        else
                            e[eLength++] = c[cInd++];

                        assignments++;
                    }

                    if (eLength == 0)
                        return d;
                }
                else
                {
                    int dInd = 0, eInd = 0;
                    int bcFlag = 0;

                    bLength = 0;
                    cLength = 0;

                    while (dInd < dLength && eInd < eLength)
                    {
                        int dSeriesStart = dInd;
                        int dSeriesEnd = dSeriesStart;

                        while (dSeriesEnd + 1 < dLength && d[dSeriesEnd] <= d[dSeriesEnd + 1])
                        {
                            comparisons++;
                            dSeriesEnd++;
                        }

                        if (dSeriesEnd + 1 < dLength)
                            comparisons++;

                        int eSeriesStart = eInd;
                        int eSeriesEnd = eSeriesStart;

                        while (eSeriesEnd + 1 < eLength && e[eSeriesEnd] <= e[eSeriesEnd + 1])
                        {
                            comparisons++;
                            eSeriesEnd++;
                        }

                        if (eSeriesEnd + 1 < eLength)
                            comparisons++;

                        int i = dSeriesStart;
                        int j = eSeriesStart;

                        if (bcFlag % 2 == 0)
                        {
                            while (i <= dSeriesEnd && j <= eSeriesEnd)
                            {
                                comparisons++;

                                if (d[i] <= e[j])
                                    b[bLength++] = d[i++];
                                else
                                    b[bLength++] = e[j++];

                                assignments++;
                            }

                            while (i <= dSeriesEnd)
                            {
                                b[bLength++] = d[i++];
                                assignments++;
                            }

                            while (j <= eSeriesEnd)
                            {
                                b[bLength++] = e[j++];
                                assignments++;
                            }
                        }
                        else
                        {
                            while (i <= dSeriesEnd && j <= eSeriesEnd)
                            {
                                comparisons++;

                                if (d[i] <= e[j])
                                    c[cLength++] = d[i++];
                                else
                                    c[cLength++] = e[j++];

                                assignments++;
                            }

                            while (i <= dSeriesEnd)
                            {
                                c[cLength++] = d[i++];
                                assignments++;
                            }

                            while (j <= eSeriesEnd)
                            {
                                c[cLength++] = e[j++];
                                assignments++;
                            }
                        }

                        dInd = dSeriesEnd + 1;
                        eInd = eSeriesEnd + 1;
                        bcFlag++;
                    }

                    while (dInd < dLength)
                    {
                        if (bcFlag % 2 == 0)
                            b[bLength++] = d[dInd++];
                        else
                            c[cLength++] = d[dInd++];

                        assignments++;
                    }

                    while (eInd < eLength)
                    {
                        if (bcFlag % 2 == 0)
                            b[bLength++] = e[eInd++];
                        else
                            c[cLength++] = e[eInd++];

                        assignments++;
                    }

                    if (cLength == 0)
                        return b;
                }

                series++;
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

            if (Convert.ToBoolean(dataGridView1.Rows[3].Cells[0].Value))
            {
                int[] sortingArray = (int[])source.Clone();
                comparisons = 0;
                assignments = 0;

                int t1 = Environment.TickCount;
                sortingArray = OnePhaseNaturalMergeSort(sortingArray, ref comparisons, ref assignments);
                int time = Environment.TickCount - t1;

                dataGridView1.Rows[3].Cells[2].Value = comparisons;
                dataGridView1.Rows[3].Cells[3].Value = assignments;
                dataGridView1.Rows[3].Cells[4].Value = time;
                dataGridView1.Rows[3].Cells[5].Value = IsSorted(sortingArray) ? "Да" : "Нет";
            }
            else
            {
                dataGridView1.Rows[3].Cells[2].Value = "";
                dataGridView1.Rows[3].Cells[3].Value = "";
                dataGridView1.Rows[3].Cells[4].Value = "";
                dataGridView1.Rows[3].Cells[5].Value = "";
            }
        }
    }
}
