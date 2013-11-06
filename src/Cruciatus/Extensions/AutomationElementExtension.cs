﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutomationElementExtension.cs" company="2GIS">
//   Cruciatus
// </copyright>
// <summary>
//   Представляет расширения для AutomationElement.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Cruciatus.Extensions
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Automation;

    using Microsoft.VisualStudio.TestTools.UITesting;

    public static class AutomationElementExtension
    {
        private const int WaitForReadyTimeout = 5000;

        public static bool WaitForElementReady(this AutomationElement element)
        {
            var walker = new TreeWalker(Condition.TrueCondition);
            AutomationElement parent = element;
            WindowPattern windowPattern = null;
            while (parent != null)
            {
                object pattern;
                if (parent.TryGetCurrentPattern(WindowPattern.Pattern, out pattern))
                {
                    windowPattern = (WindowPattern)pattern;
                    break;
                }

                parent = walker.GetParent(parent);
            }

            if (windowPattern == null)
            {
                // Теоретически такой ситуации не может быть
                // но если что, то считаем, что все ок
                return true;
            }

            // результат от WaitForInputIdle желательно проверить самостоятельно
            // ошибка при возврате false точно встречается
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            bool b = windowPattern.WaitForInputIdle(WaitForReadyTimeout);
            stopwatch.Stop();

            // Если результат true и время таймаута не вышло
            if (b && stopwatch.ElapsedMilliseconds < WaitForReadyTimeout)
            {
                return true;
            }

            // Если результат false и время таймаута вышло
            if (!b && stopwatch.ElapsedMilliseconds > WaitForReadyTimeout)
            {
                return false;
            }

            // Иначе используем UITesting
            var control = UITestControlFactory.FromNativeElement(element, "UIA");
            return control.WaitForControlReady(WaitForReadyTimeout);
        }

        public static bool GeometricallyContains(this AutomationElement externalElement, AutomationElement internaleElement)
        {
            if (!externalElement.GetSupportedProperties().Contains(AutomationElement.BoundingRectangleProperty))
            {
                // TODO Исключение вида - контрол не поддерживает свойство BoundingRectangle
                throw new Exception("внешний элемент в GeometricallyContains не поддерживает свойство BoundingRectangle");
            }

            if (!internaleElement.GetSupportedProperties().Contains(AutomationElement.BoundingRectangleProperty))
            {
                // TODO Исключение вида - контрол не поддерживает свойство BoundingRectangle
                throw new Exception("внутренний элемент в GeometricallyContains не поддерживает свойство BoundingRectangle");
            }

            var externalRect = (Rect)externalElement.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);
            var internaleRect = (Rect)internaleElement.GetCurrentPropertyValue(AutomationElement.BoundingRectangleProperty);

            return externalRect.Contains(internaleRect);
        }
    }
}
