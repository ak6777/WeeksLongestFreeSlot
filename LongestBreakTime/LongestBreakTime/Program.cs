using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LongestBreakTime
{
    class Program
    {
        static void Main(string[] args)
        {
            string userInput = @"Sun 10:00-20:00
                                 Fri 05:00-10:00
                                 Fri 16:30-23:50
                                 Sat 10:00-24:00
                                 Sun 01:00-04:00
                                 Sat 02:00-06:00
                                 Tue 03:30-18:15
                                 Tue 19:00-20:00
                                 Wed 04:25-15:14
                                 Wed 15:14-22:40
                                 Thu 00:00-23:59
                                 Mon 05:00-13:00
                                 Mon 15:00-21:00";

            string[] weeksArray = new string[] { "MON", "TUE", "WED", "THU", "FRI", "SAT", "SUN" };
            Dictionary<string, List<TimeSpan>> userScheduleDict = new Dictionary<string, List<TimeSpan>>();
            List<TimeSpan> longestSleepSlot = null;

            foreach(var item in weeksArray)
            {
                userScheduleDict.Add(item, new List<TimeSpan>());
            }
            HandleUserSchedule hus = new HandleUserSchedule();
            try
            {
                hus.GetUserScheduleDict(userInput, ref userScheduleDict);
                longestSleepSlot = hus.SortandNormalizeTimes(ref userScheduleDict, weeksArray);

                if (longestSleepSlot != null)
                    Console.WriteLine("Longest sleep slot geroge can get is {0} for the given week", longestSleepSlot.Max().TotalMinutes.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine(e.InnerException);
            }
            

            Console.ReadLine();
        }
    }

    public class HandleUserSchedule
    {
        TimeSpan defaultStartTime = TimeSpan.Parse("00:00");
        TimeSpan defaultEndTime = TimeSpan.Parse("23:59");
        List<TimeSpan> idealSleepTimeArr = null;
        List<TimeSpan> finalSleepTimeArr = new List<TimeSpan>();

        public void GetUserScheduleDict(string meetingSchedule, ref Dictionary<string, List<TimeSpan>> userScheduleDict)
        {
            //Dictionary<string, List<TimeSpan>> scheduleDict = new Dictionary<string, List<TimeSpan>>();

            if (!String.IsNullOrWhiteSpace(meetingSchedule))
            {
                foreach(string meeting in meetingSchedule.Split(new[] { '\r','\n' }))
                {
                    if (String.IsNullOrEmpty(meeting))
                    {
                        continue;
                    }

                    String[] meetingArr = meeting.Trim().Replace(' ', '-').Split('-');
                    TimeSpan meetingTime = new TimeSpan();
                    List<TimeSpan> timesList = new List<TimeSpan>();

                    if(TimeSpan.TryParse(meetingArr[1], out meetingTime))
                    {
                        timesList.Add(meetingTime);
                    }
                    else
                    {
                        Console.WriteLine("No Start Time");
                    }

                    if (meetingArr[2].ToString().Equals("24:00"))
                    {
                        meetingArr[2] = "23:59";
                        Console.WriteLine("Given time span exceeds the limit of 23:59. Auto adjusted to 23:59 for week {0}", meetingArr[0].ToUpper());
                    }

                    if(TimeSpan.TryParse(meetingArr[2], out meetingTime))
                    {
                        timesList.Add(meetingTime);
                    }
                    else
                    {
                        Console.WriteLine("No End Time");
                    }

                    if(timesList[0] > timesList[1])
                    {
                        Console.WriteLine("Start Time cannot be greater than End Time");
                    }


                    List<TimeSpan> existing = new List<TimeSpan>();
                    userScheduleDict.TryGetValue(meetingArr[0].ToUpper(), out existing);
                    existing.AddRange(timesList);

                    //if (existing == null || existing.Count == 0)
                    //{
                    //    userScheduleDict.Add(meetingArr[0].ToUpper(), timesList);
                    //}
                    //else
                    //{
                    //    existing.AddRange(timesList);
                    //}
                }
            }

        }

        public List<TimeSpan> SortandNormalizeTimes(ref Dictionary<string, List<TimeSpan>> userSchedule, string[] weeksArr)
        {
            foreach(var week in weeksArr)
            {
                if(userSchedule[week].ToList().Count == 0)
                {
                    userSchedule[week].AddRange(new List<TimeSpan> { defaultStartTime });
                    userSchedule[week].AddRange(new List<TimeSpan> { defaultEndTime });
                }
                userSchedule[week] = userSchedule[week].OrderBy(e => e.Hours).ToList();

                idealSleepTimeArr = FindIdealSleepTime(userSchedule[week]);

                if(idealSleepTimeArr != null && idealSleepTimeArr.Count > 0)
                {
                    if(finalSleepTimeArr.Count > 0 && idealSleepTimeArr.ElementAt(0) != TimeSpan.Parse("00:00"))
                    {
                        idealSleepTimeArr[0] = finalSleepTimeArr.ElementAt(finalSleepTimeArr.Count - 1) + idealSleepTimeArr.ElementAt(0);
                        finalSleepTimeArr.RemoveAt(finalSleepTimeArr.Count - 1);
                    }
                }
                
                finalSleepTimeArr.AddRange(idealSleepTimeArr);
            }

            return finalSleepTimeArr;
        }

        public List<TimeSpan> FindIdealSleepTime(List<TimeSpan> schedule)
        {
            List<TimeSpan> arrFreeTime = new List<TimeSpan>();
            if(schedule != null)
            {
                schedule.Insert(0, defaultStartTime);
                schedule.Insert(schedule.Count, defaultEndTime);

                for(int i = 0; i < schedule.Count; i++)
                {
                    TimeSpan fromTime = schedule[i];
                    TimeSpan toTime = schedule[i + 1];

                    arrFreeTime.Add(toTime - fromTime);

                    i++;
                }

                #region commented
                //if (schedule.Count == 2)
                //{
                //    //if (schedule[0] == defaultStartTime && schedule[1] == defaultEndTime)
                //    //{
                //    //    arrFreeTime.Add(schedule[1] - schedule[0]);
                //    //}

                //    arrFreeTime.Add(schedule[1] - schedule[0]);
                //}
                //else
                //{
                //    for (int i = 0; i < schedule.Count; i++)
                //    {
                //        TimeSpan fromTime = schedule[i];
                //        TimeSpan toTime = defaultEndTime;

                //        if (i != 0 && i != schedule.Count)
                //        {
                //            toTime = schedule[i + 1];
                //        }

                //        if (i == 0)
                //        {
                //            if (fromTime != defaultStartTime)
                //            {
                //                arrFreeTime.Add(fromTime - defaultStartTime);
                //            }
                //        }

                //        if (i > 0)
                //        {
                //            arrFreeTime.Add(toTime - fromTime);
                //            i++;
                //        }                        
                //    }
                //}
                #endregion
            }

            return arrFreeTime;
        }
    }
}
