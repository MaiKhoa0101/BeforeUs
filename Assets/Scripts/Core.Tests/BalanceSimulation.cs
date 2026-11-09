using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using BeforeUs.Core;
using BeforeUs.Core.Config;

namespace BeforeUs.Core.Tests
{
    public enum DeathCause
    {
        None,
        Starvation,
        Hunt,
        Explore,
        Raid,
        Opportunity,
        Unknown
    }

    public struct GameResult
    {
        public bool Won;
        public bool TimedOut;
        public int Turns;
        public int FinalStage;
        public DeathCause Cause;

        public static GameResult Win(int turns)
        {
            return new GameResult { Won = true, Turns = turns, FinalStage = 0, Cause = DeathCause.None };
        }

        public static GameResult Loss(int turns, int stage, DeathCause cause)
        {
            return new GameResult { Won = false, Turns = turns, FinalStage = stage, Cause = cause };
        }

        public static GameResult Timeout(int turns, int stage)
        {
            return new GameResult { Won = false, TimedOut = true, Turns = turns, FinalStage = stage, Cause = DeathCause.None };
        }
    }

    /// <summary>Số liệu của một lần chạy 2.000 ván.</summary>
    public sealed class BalanceStats
    {
        public string Label;
        public BotStyle Style;
        public int Games;
        public int Wins;
        public int Timeouts;
        public double MedianWinTurns;
        public double P10WinTurns;
        public double P90WinTurns;
        public int D11Saves;
        public int GamesWithD11Save;
        public Dictionary<DeathCause, int> Causes = new Dictionary<DeathCause, int>();

        /// <summary>
        /// Số ván THUA trước lượt 5 và trước lượt 10 (Supervisor đặt 2026-11-09).
        ///
        /// Vì sao ngang hàng với tỉ lệ thắng: dải Cổng 2 là điều kiện CẦN, không phải điều kiện ĐỦ.
        /// Một cấu hình thắng 35% mà một phần ba số ván chết trước lượt 5 thì vẫn hỏng — người chơi
        /// thua trước khi kịp hiểu luật, trong khi design.md §1 đặt cảm xúc mục tiêu là
        /// "thua thì bực, muốn chơi lại", không phải "chết trước khi thấy được gì".
        ///
        /// Lead UI tìm ra bằng hạt 12345: bộ tộc tuyệt diệt ở lượt 3, hai lượt đói liên tiếp.
        /// </summary>
        public int LossesBeforeTurn5;

        public int LossesBeforeTurn10;

        /// <summary>Nguyên nhân thua, CHỈ tính những ván thua trong 10 lượt đầu.</summary>
        public Dictionary<DeathCause, int> EarlyCauses = new Dictionary<DeathCause, int>();

        public double EarlyLossRate5
        {
            get { return Games == 0 ? 0 : (double)LossesBeforeTurn5 / Games; }
        }

        public double EarlyLossRate10
        {
            get { return Games == 0 ? 0 : (double)LossesBeforeTurn10 / Games; }
        }

        public double WinRate
        {
            get { return (double)Wins / Games; }
        }

        /// <summary>Sai số chuẩn của tỉ lệ thắng: sqrt(p(1-p)/n).</summary>
        public double StandardError
        {
            get { return Math.Sqrt(WinRate * (1.0 - WinRate) / Games); }
        }

        public string WinRateText
        {
            get
            {
                return (WinRate * 100).ToString("F1", CultureInfo.InvariantCulture)
                       + "% ± " + (StandardError * 100).ToString("F1", CultureInfo.InvariantCulture);
            }
        }
    }

    /// <summary>Chạy nhiều ván và gom số liệu. C8 so THỐNG KÊ với mô phỏng, không so chuỗi.</summary>
    public static class BalanceSimulation
    {
        public const int DefaultGames = 2000;
        public const int BaseSeed = 1000;

        public static BalanceStats Run(BalanceConfig config, BotOptions options, string label, int games = DefaultGames)
        {
            BalanceStats stats = new BalanceStats
            {
                Label = label,
                Style = options.Style,
                Games = games
            };

            List<int> winTurns = new List<int>();

            for (int i = 0; i < games; i++)
            {
                BotGame game = new BotGame(config, options, BaseSeed + i);
                GameResult result = game.Play();

                if (game.D11Saves > 0)
                {
                    stats.GamesWithD11Save++;
                    stats.D11Saves += game.D11Saves;
                }

                if (result.Won)
                {
                    stats.Wins++;
                    winTurns.Add(result.Turns);
                    continue;
                }

                if (result.TimedOut)
                {
                    stats.Timeouts++;
                    continue;
                }

                int current;
                stats.Causes.TryGetValue(result.Cause, out current);
                stats.Causes[result.Cause] = current + 1;

                if (result.Turns < 10)
                {
                    stats.LossesBeforeTurn10++;

                    int early;
                    stats.EarlyCauses.TryGetValue(result.Cause, out early);
                    stats.EarlyCauses[result.Cause] = early + 1;

                    if (result.Turns < 5)
                    {
                        stats.LossesBeforeTurn5++;
                    }
                }
            }

            winTurns.Sort();
            stats.MedianWinTurns = Quantile(winTurns, 0.5);
            stats.P10WinTurns = Quantile(winTurns, 0.1);
            stats.P90WinTurns = Quantile(winTurns, 0.9);
            return stats;
        }

        /// <summary>Nội suy tuyến tính trên dãy đã sắp xếp. Trả 0 nếu không có ván thắng nào.</summary>
        private static double Quantile(List<int> sorted, double q)
        {
            if (sorted.Count == 0)
            {
                return 0;
            }

            if (sorted.Count == 1)
            {
                return sorted[0];
            }

            double position = q * (sorted.Count - 1);
            int low = (int)Math.Floor(position);
            int high = (int)Math.Ceiling(position);
            double weight = position - low;
            return sorted[low] * (1 - weight) + sorted[high] * weight;
        }

        public static string Table(string title, IReadOnlyList<BalanceStats> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendLine("=== " + title + " ===");
            sb.AppendLine("bot        thắng           trung vị   p10    p90    <5     <10    hết lượt  nguyên nhân thua");

            foreach (BalanceStats row in rows)
            {
                sb.Append(row.Style.ToString().ToLowerInvariant().PadRight(11));
                sb.Append(row.WinRateText.PadRight(16));
                sb.Append(Num(row.MedianWinTurns).PadRight(11));
                sb.Append(Num(row.P10WinTurns).PadRight(7));
                sb.Append(Num(row.P90WinTurns).PadRight(7));
                sb.Append(Pct(row.EarlyLossRate5).PadRight(7));
                sb.Append(Pct(row.EarlyLossRate10).PadRight(7));
                sb.Append(row.Timeouts.ToString(CultureInfo.InvariantCulture).PadRight(10));
                sb.Append(CausesText(row));
                sb.AppendLine();
            }

            sb.AppendLine("  <5 / <10 = tỉ lệ ván THUA trước lượt 5 / lượt 10. Ngưỡng [ĐỀ XUẤT]: balanced <10 ≤ 10%.");
            sb.Append(EarlyCausesBlock(rows));

            return sb.ToString();
        }

        private static string Num(double value)
        {
            return value <= 0 ? "-" : value.ToString("F1", CultureInfo.InvariantCulture);
        }

        private static string Pct(double value)
        {
            return (value * 100).ToString("F1", CultureInfo.InvariantCulture) + "%";
        }

        /// <summary>
        /// Nguyên nhân thua TÁCH RIÊNG cho 10 lượt đầu. Gộp chung với cả ván thì một mở màn
        /// chí mạng bị pha loãng mất — đúng cái đã xảy ra với starvation_deaths = 2.
        /// </summary>
        private static string EarlyCausesBlock(IReadOnlyList<BalanceStats> rows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("  nguyên nhân thua TRONG 10 LƯỢT ĐẦU:");

            foreach (BalanceStats row in rows)
            {
                sb.Append("    " + row.Style.ToString().ToLowerInvariant().PadRight(11));

                if (row.LossesBeforeTurn10 == 0)
                {
                    sb.AppendLine("không ván nào thua sớm");
                    continue;
                }

                foreach (DeathCause cause in new[]
                {
                    DeathCause.Starvation, DeathCause.Hunt, DeathCause.Explore,
                    DeathCause.Raid, DeathCause.Opportunity, DeathCause.Unknown
                })
                {
                    int count;
                    if (row.EarlyCauses.TryGetValue(cause, out count) && count > 0)
                    {
                        sb.Append(cause.ToString().ToLowerInvariant() + "="
                                  + Pct((double)count / row.Games) + "  ");
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static string CausesText(BalanceStats stats)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DeathCause cause in new[]
            {
                DeathCause.Starvation, DeathCause.Raid, DeathCause.Hunt,
                DeathCause.Explore, DeathCause.Opportunity, DeathCause.Unknown
            })
            {
                int count;
                if (!stats.Causes.TryGetValue(cause, out count) || count == 0)
                {
                    continue;
                }

                if (sb.Length > 0)
                {
                    sb.Append("  ");
                }

                sb.Append(cause.ToString().ToLowerInvariant()).Append('=')
                  .Append((100.0 * count / stats.Games).ToString("F1", CultureInfo.InvariantCulture)).Append('%');
            }

            return sb.ToString();
        }
    }
}
