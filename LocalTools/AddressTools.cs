using System;
using System.Collections.Generic;
using System.Linq;
using Tools;

namespace BankService.LocalTools
{
    public static class AddressTools
    {
        //TODO change name, can be used not only to addresses
        public static List<string> SplitAddress(string address, int elementsCount, int maxElementLength)
        {
            //TODO option split equally, regardless separators characters

            List<string> linesResult = null;

            int repairIndex = -1;

            while (linesResult == null)
            {
                List<string> lines = new List<string>();

                bool reachedEnd = false;
                int previousEnd = 0;
                int end = maxElementLength;

                if (string.IsNullOrEmpty(address))
                    reachedEnd = true;

                for (int i = 0; i < elementsCount; i++)
                {
                    if (!reachedEnd)
                    {
                        //TODO as option
                        while (previousEnd < address.Length && address[previousEnd] == ' ')
                        {
                            previousEnd++;
                            end++;
                        }

                        if (end > address.Length)
                            end = address.Length;

                        int breakIndex;
                        if (end >= address.Length)
                            breakIndex = address.Length - 1;
                        else
                        {
                            if (i <= repairIndex)
                                breakIndex = end;
                            else
                                breakIndex = address.IndexOfFirstEx(StringOperations.separatorChars.Select(c => c.ToString()), end, false, true);
                        }

                        if (breakIndex == -1 || breakIndex <= previousEnd)
                            breakIndex = end;
                        if (breakIndex != end && address[breakIndex] != ' ')
                            breakIndex++;

                        string result = address.Substring(previousEnd, breakIndex - previousEnd);
                        lines.Add(result);

                        if (breakIndex >= address.Length)
                            reachedEnd = true;

                        previousEnd = breakIndex;
                        end = breakIndex + maxElementLength;
                    }
                    else
                        lines.Add(String.Empty);
                }

                if (!reachedEnd && previousEnd < address.TrimEnd().Length)
                {
                    repairIndex++;
                    if (repairIndex == elementsCount)
                        linesResult = lines;
                }
                else
                    linesResult = lines;
            }

            return linesResult;
        }
    }
}
