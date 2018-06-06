using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace TheTennisProject
{
    /// <summary>
    /// Conversion d'un résultat de match (vainqueur / vaincu) en couleur.
    /// </summary>
    public class WinnerToColourConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convertit un résultat de match en couleur.
        /// </summary>
        /// <param name="values">Le vainqueur du match, puis l'élément visuel appelant (ce dernier contient le joueur à comparer dans sa propriété Tag).</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>La couleur associée au résultat du match.</returns>
        /// <exception cref="ArgumentException">Le tableau de paramètres est invalide.</exception>
        /// <exception cref="ArgumentException">Le premier élément du tableau n'est pas un joueur valide.</exception>
        /// <exception cref="ArgumentException">Le second élément du tableau doit être le composant visuel appelant.</exception>
        /// <exception cref="InvalidOperationException">Impossible de récupérer le composant parent.</exception>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 2)
            {
                throw new ArgumentException("Le tableau de paramètres est invalide.", "values");
            }

            if (values[0] == null || values[0].GetType() != typeof(Services.Player))
            {
                throw new ArgumentException("Le premier élément du tableau n'est pas un joueur valide.", "values");
            }

            if (values[1] == null || !values[1].GetType().IsSubclassOf(typeof(DependencyObject)))
            {
                throw new ArgumentException("Le second élément du tableau doit être le composant visuel appelant.", "values");
            }

            System.Windows.Controls.ListView listViewControl = null;
            DependencyObject currentControl = (DependencyObject)values[1];
            while (listViewControl == null)
            {
                currentControl = VisualTreeHelper.GetParent(currentControl);
                if (currentControl == null)
                {
                    throw new InvalidOperationException("Impossible de récupérer le composant parent.");
                }
                listViewControl = currentControl as System.Windows.Controls.ListView;
            }
            
            // TODO : pinceaux à configurer proprement.
            return listViewControl.Tag == values[0] ? Brushes.Green : Brushes.Red;
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetTypes">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">La méthode n'est pas implémentée.</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion en libellé de l'adversaire du joueur dont les matchs sont liés au composant.
    /// </summary>
    public class OpponentNameConverter : IMultiValueConverter
    {
        /// <summary>
        /// Convertit un adversaire en libellé.
        /// </summary>
        /// <param name="values">Le vainqueur du match, puis le vaincu du match, enfin l'élément visuel appelant (ce dernier contient le joueur à comparer dans sa propriété Tag).</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>Le nom de l'adversaire.</returns>
        /// <exception cref="ArgumentException">Le tableau de paramètres est invalide.</exception>
        /// <exception cref="ArgumentException">Le premier élément du tableau n'est pas un joueur valide.</exception>
        /// <exception cref="ArgumentException">Le second élément du tableau n'est pas un joueur valide.</exception>
        /// <exception cref="ArgumentException">Le troisième élément du tableau doit être le composant visuel appelant.</exception>
        /// <exception cref="InvalidOperationException">Impossible de récupérer le composant parent.</exception>
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 3)
            {
                throw new ArgumentException("Le tableau de paramètres est invalide.", "values");
            }

            if (values[0] == null || values[0].GetType() != typeof(Services.Player))
            {
                throw new ArgumentException("Le premier élément du tableau n'est pas un joueur valide.", "values");
            }

            if (values[1] == null || values[1].GetType() != typeof(Services.Player))
            {
                throw new ArgumentException("Le second élément du tableau n'est pas un joueur valide.", "values");
            }

            if (values[2] == null || !values[2].GetType().IsSubclassOf(typeof(DependencyObject)))
            {
                throw new ArgumentException("Le troisième argument doit être le composant visuel appelant.", "values");
            }

            System.Windows.Controls.ListView listViewControl = null;
            DependencyObject currentControl = (DependencyObject)values[2];
            while (listViewControl == null)
            {
                currentControl = VisualTreeHelper.GetParent(currentControl);
                if (currentControl == null)
                {
                    throw new InvalidOperationException("Impossible de récupérer le composant parent.");
                }
                listViewControl = currentControl as System.Windows.Controls.ListView;
            }

            Services.Player pWin = values[0] as Services.Player;
            Services.Player pLos = values[1] as Services.Player;
            Services.Player pMe = listViewControl.Tag as Services.Player;

            return pWin == pMe ? pLos.Name : pWin.Name;
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetTypes">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">La méthode n'est pas implémentée.</exception>
        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion d'une valeur d'énumération <see cref="Services.Level"/> en sa traduction.
    /// </summary>
    public class LevelTranslationConverter : IValueConverter
    {
        /// <summary>
        /// Convertit une valeur d'énumération <see cref="Services.Level"/> en sa traduction.
        /// </summary>
        /// <param name="value">La valeur d'énumération <see cref="Services.Level"/> à traduire.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>La valeur traduite.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(Services.Level))
            {
                return string.Empty;
            }

            return ((Services.Level)value).GetTranslation();
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion d'une valeur d'énumération <see cref="Services.Surface"/> en sa traduction.
    /// </summary>
    public class SurfaceTranslationConverter : IValueConverter
    {
        /// <summary>
        /// Convertit une valeur d'énumération <see cref="Services.Surface"/> en sa traduction.
        /// </summary>
        /// <param name="value">La valeur d'énumération <see cref="Services.Surface"/> à traduire.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>La valeur traduite.</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(Services.Surface))
            {
                return string.Empty;
            }

            return ((Services.Surface)value).GetTranslation();
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion d'un booléen "Environnement indoor" en texte.
    /// </summary>
    public class IndoorTextConverter : IValueConverter
    {
        /// <summary>
        /// Convertit un booléen "Environnement indoor" en texte.
        /// </summary>
        /// <param name="value">La valeur booléenne indiquant l'environnement indoor.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>Le texte associé à la valeur booléenne.</returns>
        /// <exception cref="ArgumentException">L'argument est null ou d'un autre type que celui attendu.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(bool))
            {
                throw new ArgumentException("L'argument est null ou d'un autre type que celui attendu.", nameof(value));
            }

            return ((bool)value) ? "Indoor" : string.Empty;
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion des positions de classement pour affichage (texte). 
    /// </summary>
    public class RankingTextConverter : IValueConverter
    {
        /// <summary>
        /// Convertit une position dans un classement en son équivalent texte sur 2 caractères.
        /// </summary>
        /// <param name="value">La valeur numérique du classement.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>Le texte associé au classement.</returns>
        /// <exception cref="ArgumentException">L'argument est null ou d'un autre type que celui attendu.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(uint))
            {
                throw new ArgumentException("L'argument est null ou d'un autre type que celui attendu.", nameof(value));
            }

            return value.ToString().PadLeft(2, '0');
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion des positions de classement pour affichage (graphique). 
    /// </summary>
    public class RankingToMedalConverter : IValueConverter
    {
        /// <summary>
        /// Convertit une position dans un classement en une couleur (<see cref="Colors"/>) pour l'affichage en médaillon.
        /// </summary>
        /// <param name="value">La valeur numérique du classement.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>La couleur associée au classement.</returns>
        /// <exception cref="ArgumentException">L'argument est null ou d'un autre type que celui attendu.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(uint))
            {
                throw new ArgumentException("L'argument est null ou d'un autre type que celui attendu.", nameof(value));
            }

            switch ((uint)value)
            {
                case 1:
                    return Colors.Gold;
                case 2:
                    return Colors.Silver;
                case 3:
                    return Colors.Peru;
                default:
                    return Colors.Lavender;
            }
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Conversion des points ATP pour affichage (largeur de canvas / panel).
    /// </summary>
    public class PointsToWidthConverter : IValueConverter
    {
        /// <summary>
        /// Convertit des points ATP en un dimension (largeur) de panel ou canvas.
        /// </summary>
        /// <param name="value">La valeur numérique des points.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>La largeur du panel/canvas associé aux points.</returns>
        /// <exception cref="ArgumentException">L'argument est null ou d'un autre type que celui attendu.</exception>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value.GetType() != typeof(uint))
            {
                throw new ArgumentException("L'argument est null ou d'un autre type que celui attendu.", nameof(value));
            }

            return 150 + ((((uint)value) * 300) / (double)18000);
        }

        /// <summary>
        /// Réciproque de conversion. Non implémentée.
        /// </summary>
        /// <param name="value">N/A.</param>
        /// <param name="targetType">N/A.</param>
        /// <param name="parameter">N/A.</param>
        /// <param name="culture">N/A.</param>
        /// <returns>N/A.</returns>
        /// <exception cref="NotImplementedException">Cette fonction n'est pas implémentée.</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
