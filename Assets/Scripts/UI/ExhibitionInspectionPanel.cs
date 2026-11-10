using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MyProstateRejectsCapitalism.UI
{
    internal sealed class ExhibitionInspectionPanel
    {
        private const float Width = 594f;
        private const float Inset = 54f;
        private const float TextWidth = Width - 2f * Inset;
        private const float CardWidth = 522f;
        private const float CardInset = 36f;
        private const float CardTextWidth = CardWidth - 2f * CardInset;
        private const float CardBodyTop = 200f;
        private const float CardSideMargin = 64f;
        private static readonly Color32 Slate = new(27, 35, 39, 255);
        private static readonly Color32 Ivory = new(245, 246, 241, 255);
        private static readonly Color32 Muted = new(188, 202, 201, 255);
        private static readonly Color32 Line = new(87, 103, 106, 255);
        private static readonly Color32 Paper = new(250, 248, 241, 255);
        private static readonly Color32 Edge = new(194, 189, 176, 255);
        private static readonly Color32 Ink = new(44, 42, 37, 255);
        private static readonly Color32 SecondaryInk = new(93, 88, 78, 255);

        private readonly bool useCatalogueCard;
        private readonly ExhibitionPanelDesignArt fullHeightArt;
        private readonly ExhibitionSalesPoster salesPoster;
        private readonly RectTransform surface;
        private readonly RectTransform card;
        private readonly CanvasGroup group;
        private readonly RectTransform footerRule;
        private readonly RectTransform footer;
        private readonly TMP_Text number;
        private readonly TMP_Text title;
        private readonly TMP_Text material;
        private readonly TMP_Text description;
        internal ExhibitionPanelDesign Design { get; }

        internal ExhibitionInspectionPanel(TMP_FontAsset plaqueFont, bool useCatalogueCard, ExhibitionPanelDesign panelDesign)
        {
            this.useCatalogueCard = useCatalogueCard;
            Design = panelDesign;
            var controlFont = Resources.Load<TMP_FontAsset>(TitleMenu.FontResource);
            if (controlFont == null)
                controlFont = plaqueFont;
            var layout = MenuUi.CreateCanvas("Exhibit inspection UI", 70, out surface);
            group = surface.gameObject.AddComponent<CanvasGroup>();
            group.interactable = false;
            group.blocksRaycasts = false;

            if (!useCatalogueCard && panelDesign == ExhibitionPanelDesign.SalesPoster)
            {
                card = MenuUi.Rect(layout, "Full-height sales poster", 0, 0, Width, 1080);
                salesPoster = new ExhibitionSalesPoster(card, plaqueFont, controlFont);
                number = salesPoster.Number;
                title = salesPoster.Title;
                material = salesPoster.MaterialLine;
                description = salesPoster.Description;
                Hide();
                return;
            }

            if (!useCatalogueCard && panelDesign != ExhibitionPanelDesign.Original)
            {
                card = MenuUi.Rect(layout, "Full-height exhibition panel", 0, 0, Width, 1080);
                fullHeightArt = new ExhibitionPanelDesignArt(card, plaqueFont, controlFont, panelDesign);
                number = fullHeightArt.Number;
                title = fullHeightArt.Title;
                material = fullHeightArt.Material;
                description = fullHeightArt.Description;
                Hide();
                return;
            }

            if (useCatalogueCard)
            {
                card = Block(layout, "Catalogue card", 0, 0, CardWidth, 0, Edge);
                var shadow = card.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color32(54, 49, 42, 26);
                shadow.effectDistance = new Vector2(0f, -3f);
                var paper = Block(card, "Ivory paper", 0, 0, 0, 0, Paper);
                paper.anchorMin = Vector2.zero;
                paper.anchorMax = Vector2.one;
                paper.offsetMin = Vector2.one;
                paper.offsetMax = -Vector2.one;

                number = MenuUi.Text(card, plaqueFont, string.Empty, CardInset, 27, CardTextWidth, 28, 19, SecondaryInk);
                number.characterSpacing = 2.6f;
                Block(card, "Catalogue header rule", CardInset, 65, CardTextWidth, 1, Edge);
                title = MenuUi.Text(card, plaqueFont, string.Empty, CardInset, 82, CardTextWidth, 71, 51, Ink,
                    TextAlignmentOptions.TopLeft);
                title.fontStyle = FontStyles.Bold;
                material = MenuUi.Text(card, plaqueFont, string.Empty, CardInset, 151, CardTextWidth, 31, 22, SecondaryInk,
                    TextAlignmentOptions.TopLeft);
                description = MenuUi.Text(card, plaqueFont, string.Empty, CardInset, CardBodyTop, CardTextWidth, 0, 27, Ink,
                    TextAlignmentOptions.TopLeft);
                description.textWrappingMode = TextWrappingModes.Normal;
                description.lineSpacing = 13f;
                footerRule = Block(card, "Catalogue footer rule", CardInset, 0, CardTextWidth, 1, Edge);
                footer = MenuUi.Text(card, controlFont, "F / Esc   돌아가기", CardInset, 0, CardTextWidth, 27, 19,
                    SecondaryInk).rectTransform;
                Hide();
                return;
            }

            card = Block(layout, "Gallery information", 0, 0, Width, 1080, Slate);

            number = MenuUi.Text(card, plaqueFont, string.Empty, Inset, 155, TextWidth, 38, 24, Muted);
            number.characterSpacing = 4f;
            Block(card, "Header rule", Inset, 211, TextWidth, 2, Line);
            title = MenuUi.Text(card, plaqueFont, string.Empty, Inset, 243, TextWidth, 116, 62, Ivory,
                TextAlignmentOptions.TopLeft);
            title.fontStyle = FontStyles.Bold;
            title.textWrappingMode = TextWrappingModes.Normal;
            material = MenuUi.Text(card, plaqueFont, string.Empty, Inset, 368, TextWidth, 70, 28, Muted,
                TextAlignmentOptions.TopLeft);
            material.textWrappingMode = TextWrappingModes.Normal;
            description = MenuUi.Text(card, plaqueFont, string.Empty, Inset, 472, TextWidth, 320, 30, Ivory,
                TextAlignmentOptions.TopLeft);
            description.textWrappingMode = TextWrappingModes.Normal;
            description.lineSpacing = 18f;
            Block(card, "Footer rule", Inset, 924, TextWidth, 2, Line);
            MenuUi.Text(card, controlFont, "F / Esc   돌아가기", Inset, 952, TextWidth, 48, 27, Muted);
            Hide();
        }

        internal float Opacity { set => group.alpha = value; }

        internal void Show(string[] lines, bool left)
        {
            number.text = lines[0];
            if (salesPoster != null)
                salesPoster.SetHeadline(lines[0], lines[1]);
            else if (fullHeightArt != null)
                fullHeightArt.SetHeadline(lines[0], lines[1]);
            else
                title.text = lines[1];
            material.text = lines[2];
            // Keep Korean words intact while allowing breaks at spaces and authored newlines.
            description.text = Regex.Replace(lines[3], @"\S+", "<nobr>$0</nobr>");
            if (useCatalogueCard)
            {
                float bodyHeight = Mathf.Ceil(description.GetPreferredValues(description.text, CardTextWidth,
                    Mathf.Infinity).y) + 5f;
                float dividerTop = CardBodyTop + bodyHeight + 23f;
                float height = dividerTop + 64f;
                description.rectTransform.sizeDelta = new Vector2(CardTextWidth, bodyHeight);
                footerRule.anchoredPosition = new Vector2(CardInset, -dividerTop);
                footer.anchoredPosition = new Vector2(CardInset, -dividerTop - 12f);
                card.sizeDelta = new Vector2(CardWidth, height);
                card.anchoredPosition = new Vector2(left ? CardSideMargin : 1920f - CardSideMargin - CardWidth,
                    (height - 1080f) * .5f);
            }
            else
                card.anchoredPosition = new Vector2(left ? 0f : 1920f - Width, 0f);
            Opacity = 0f;
            surface.gameObject.SetActive(true);
            salesPoster?.PlaceInkDrops();
        }

        internal void Hide()
        {
            if (surface != null)
                surface.gameObject.SetActive(false);
        }

        internal void Destroy()
        {
            fullHeightArt?.Destroy();
            salesPoster?.Destroy();
            if (surface != null)
                Object.Destroy(surface.gameObject);
        }

        private static RectTransform Block(RectTransform parent, string name, float x, float y,
            float width, float height, Color color)
        {
            var rect = MenuUi.Rect(parent, name, x, y, width, height);
            var image = rect.gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }
    }
}
