using ScottPlot;
using Discord;
using Discord.Interactions;

namespace IndYBot.Helpers;

public class PlotHelper
{
   public static async Task SendBasicPlot(
         SocketInteractionContext context,
         List<string> xValues,
         List<double> yValues,
         string title,
         string xLabel,
         string yLabel,
         string colorHex = "#3498db",
         bool isDark = false,
         List<double>? secondYValues = null)
   {
      var xPos = Enumerable.Range(0, xValues.Count).Select(i => (double)i).ToList();

      Plot plot = new();

      if (secondYValues != null)
      {
         var secondBars = plot.Add.Bars(xPos, secondYValues);

         foreach (var secondBar in secondBars.Bars)
         {
            secondBar.FillColor = ScottPlot.Color.FromHex(colorHex).Lighten(0.4);
         }
      }

      var bars = plot.Add.Bars(xPos, yValues);

      foreach (var bar in bars.Bars)
      {
         bar.FillColor = ScottPlot.Color.FromHex(colorHex);
      }

      plot.Axes.Bottom.SetTicks(xPos.ToArray(), xValues.ToArray());
      plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
      plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.UpperRight;
      plot.Axes.Bottom.MinimumSize = 65;

      plot.Title(title);
      plot.XLabel(xLabel);
      plot.YLabel(yLabel);
      plot.Axes.Margins(bottom: 0);

      if (isDark)
      {
         plot.Axes.Color(new ("#ebebed"));
         plot.FigureBackground.Color = new ("#29292d");
      }

      plot.SavePng($"./tmpFiles/{title.ToLower()}.png", 1280, 720);
      var attachment = new FileAttachment($"./tmpFiles/{title.ToLower()}.png", $"{title.ToLower()}.png");
      await context.Channel.SendFileAsync(attachment);

      File.Delete($"./tmpFiles/{title.ToLower()}.png");
   }
}
