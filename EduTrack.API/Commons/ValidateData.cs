using TD.Lib.Common;

namespace EduTrack.API.Commons
{
    public class ValidateData
    {
        public static bool IsDuplicated<T>(ref string errorMess, IEnumerable<T> lstDatas, string fieldCheck, object valueCheck, object idValue = null)
        {
            if (lstDatas.Any(r => r.GetType().GetProperty("Id").GetValue(r, null).ToString() != idValue.ToString() && r.GetType().GetProperty(fieldCheck).GetValue(r, null).ToString() == valueCheck.ToString()))
            {
                errorMess = string.Format(MessageText.Duplicate, fieldCheck);
                return true;
            }
            else return false;
        }
    }
}
