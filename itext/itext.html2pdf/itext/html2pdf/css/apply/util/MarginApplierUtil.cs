/*
This file is part of the iText (R) project.
Copyright (c) 1998-2024 Apryse Group NV
Authors: Apryse Software.

This program is offered under a commercial and under the AGPL license.
For commercial licensing, contact us at https://itextpdf.com/sales.  For AGPL licensing, see below.

AGPL licensing:
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using iText.Commons;
using iText.Html2pdf.Attach;
using iText.Html2pdf.Css;
using iText.Html2pdf.Logs;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.StyledXmlParser.Css.Util;

namespace iText.Html2pdf.Css.Apply.Util {
    /// <summary>Utilities class to apply margins.</summary>
    public sealed class MarginApplierUtil {
        /// <summary>The logger.</summary>
        private static readonly ILogger logger = ITextLogManager.GetLogger(typeof(iText.Html2pdf.Css.Apply.Util.MarginApplierUtil
            ));

        /// <summary>
        /// Creates a
        /// <see cref="MarginApplierUtil"/>
        /// instance.
        /// </summary>
        private MarginApplierUtil() {
        }

        /// <summary>Applies margins to an element.</summary>
        /// <param name="cssProps">the CSS properties</param>
        /// <param name="context">the processor context</param>
        /// <param name="element">the element</param>
        public static void ApplyMargins(IDictionary<String, String> cssProps, ProcessorContext context, IPropertyContainer
             element) {
            ApplyMargins(cssProps, context, element, 0.0f, 0.0f);
        }

        /// <summary>Applies margins to an element.</summary>
        /// <param name="cssProps">the CSS properties</param>
        /// <param name="context">the processor context</param>
        /// <param name="element">the element</param>
        /// <param name="baseValueHorizontal">value used by default for horizontal dimension</param>
        /// <param name="baseValueVertical">value used by default for vertical dimension</param>
        public static void ApplyMargins(IDictionary<String, String> cssProps, ProcessorContext context, IPropertyContainer
             element, float baseValueVertical, float baseValueHorizontal) {
            String marginTop = cssProps.Get(CssConstants.MARGIN_TOP);
            String marginBottom = cssProps.Get(CssConstants.MARGIN_BOTTOM);
            String marginLeft = cssProps.Get(CssConstants.MARGIN_LEFT);
            String marginRight = cssProps.Get(CssConstants.MARGIN_RIGHT);
            // The check for display is useful at least for images
            bool isBlock = element is IBlockElement || CssConstants.BLOCK.Equals(cssProps.Get(CssConstants.DISPLAY));
            bool isImage = element is Image;
            float em = CssDimensionParsingUtils.ParseAbsoluteLength(cssProps.Get(CssConstants.FONT_SIZE));
            float rem = context.GetCssContext().GetRootFontSize();
            if (isBlock || isImage) {
                TrySetMarginIfNotAuto(Property.MARGIN_TOP, marginTop, element, em, rem, baseValueVertical);
                TrySetMarginIfNotAuto(Property.MARGIN_BOTTOM, marginBottom, element, em, rem, baseValueVertical);
            }
            bool isLeftAuto = !TrySetMarginIfNotAuto(Property.MARGIN_LEFT, marginLeft, element, em, rem, baseValueHorizontal
                );
            bool isRightAuto = !TrySetMarginIfNotAuto(Property.MARGIN_RIGHT, marginRight, element, em, rem, baseValueHorizontal
                );
            if (isBlock) {
                if (isLeftAuto && isRightAuto) {
                    element.SetProperty(Property.HORIZONTAL_ALIGNMENT, HorizontalAlignment.CENTER);
                }
                else {
                    if (isLeftAuto) {
                        element.SetProperty(Property.HORIZONTAL_ALIGNMENT, HorizontalAlignment.RIGHT);
                    }
                    else {
                        if (isRightAuto) {
                            element.SetProperty(Property.HORIZONTAL_ALIGNMENT, HorizontalAlignment.LEFT);
                        }
                    }
                }
            }
        }

        /// <summary>Tries set margin if the value isn't "auto".</summary>
        /// <param name="marginProperty">the margin property</param>
        /// <param name="marginValue">the margin value</param>
        /// <param name="element">the element</param>
        /// <param name="em">the em value</param>
        /// <param name="rem">the root em value</param>
        /// <param name="baseValue">value used by default</param>
        /// <returns>false if the margin value was "auto"</returns>
        private static bool TrySetMarginIfNotAuto(int marginProperty, String marginValue, IPropertyContainer element
            , float em, float rem, float baseValue) {
            bool isAuto = CssConstants.AUTO.Equals(marginValue);
            if (isAuto) {
                return false;
            }
            float? marginVal = ParseMarginValue(marginValue, em, rem, baseValue);
            if (marginVal != null) {
                element.SetProperty(marginProperty, UnitValue.CreatePointValue((float)marginVal));
            }
            return true;
        }

        /// <summary>Parses the margin value.</summary>
        /// <param name="marginValString">
        /// the margin value as a
        /// <see cref="System.String"/>
        /// </param>
        /// <param name="em">the em value</param>
        /// <param name="rem">the root em value</param>
        /// <param name="baseValue">value used my default</param>
        /// <returns>
        /// the margin value as a
        /// <see cref="float?"/>
        /// </returns>
        private static float? ParseMarginValue(String marginValString, float em, float rem, float baseValue) {
            UnitValue marginUnitVal = CssDimensionParsingUtils.ParseLengthValueToPt(marginValString, em, rem);
            if (marginUnitVal != null) {
                if (!marginUnitVal.IsPointValue()) {
                    if (baseValue != 0.0f) {
                        return System.Convert.ToSingle(baseValue * marginUnitVal.GetValue() * 0.01, System.Globalization.CultureInfo.InvariantCulture
                            );
                    }
                    logger.LogError(Html2PdfLogMessageConstant.MARGIN_VALUE_IN_PERCENT_NOT_SUPPORTED);
                    return null;
                }
                return marginUnitVal.GetValue();
            }
            else {
                return null;
            }
        }
    }
}
