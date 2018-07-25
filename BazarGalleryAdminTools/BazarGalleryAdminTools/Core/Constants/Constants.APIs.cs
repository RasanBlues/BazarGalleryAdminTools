using System;
using System.Collections.Generic;
using System.Text;

namespace BazarGallery
{
    public static partial class Constants
    {
        public const string EnglishTranslationID = "en;";
        public const string ArabicTranslationID = "ar;";

        public const string GetCategoriesAPI = "http://bazargallery.com/api/apis/categoryList?lang=ku";
        public const string GetSubcategoriesAPI = "http://bazargallery.com/api/apis/subcategoryList?lang=ku&cat_id={0}";
        public const string GetAllSubcategoriesAPI = "http://bazargallery.com/api/apis/subcategoryList?lang=ku";
        public const string GetAllKeywordsAPI = "http://bazargallery.com/api/apis/keyboardList?lang=ku&keyword=";
    }
}
