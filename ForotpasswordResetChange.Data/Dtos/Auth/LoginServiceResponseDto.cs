using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForotpasswordResetChange.Data.Dtos.Auth
{
    public class LoginServiceResponseDto
    {
        public string NewToken { get; set; } = string.Empty;
        public bool IsSucceed { get; set; }
        public string Message {  get; set; }

        // This would be returned to front-end
        public UserInfoResult UserInfo { get; set; } = new UserInfoResult();
    }
}
