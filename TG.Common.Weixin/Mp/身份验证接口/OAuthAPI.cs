﻿/*----------------------------------------------------------------
    Copyright (C) 2015 Senparc
    
    文件名：OAuthAPI.cs
    文件功能描述：OAuth
    
    
    创建标识：Senparc - 20150211
    
    修改标识：Senparc - 20150303
    修改描述：整理接口
----------------------------------------------------------------*/

/*
    官方文档：http://mp.weixin.qq.com/wiki/index.php?title=%E7%BD%91%E9%A1%B5%E6%8E%88%E6%9D%83%E8%8E%B7%E5%8F%96%E7%94%A8%E6%88%B7%E5%9F%BA%E6%9C%AC%E4%BF%A1%E6%81%AF#.E7.AC.AC.E4.B8.80.E6.AD.A5.EF.BC.9A.E7.94.A8.E6.88.B7.E5.90.8C.E6.84.8F.E6.8E.88.E6.9D.83.EF.BC.8C.E8.8E.B7.E5.8F.96code
 */



using TG.Common.Weixin.Mp;
using TG.Common.Weixin.Mp.Datas;
using System.Web;

namespace TG.Common.Weixin.Mp
{
    public class OAuthApi : MpApi
    {
        public OAuthApi(WxMpApi api) : base(api) { }
        /// <summary>
        /// 获取验证地址
        /// </summary>
        /// <param name="redirectUrl"></param>
        /// <param name="state"></param>
        /// <param name="scope"></param>
        /// <param name="responseType"></param>
        /// <returns></returns>
        public string GetAuthorizeUrl(string redirectUrl = "", string state = "STATE", OAuthScope scope = OAuthScope.snsapi_base, string responseType = "code")
        {
            if (string.IsNullOrEmpty(redirectUrl))
            {
                redirectUrl = HttpContext.Current.Request.Url.ToString();
            }

            var appId = _api.GetAppid();
            var url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type={2}&scope={3}&state={4}#wechat_redirect",
                            appId, HttpUtility.UrlEncode(redirectUrl), responseType, scope, state);

            /* 这一步发送之后，客户会得到授权页面，无论同意或拒绝，都会返回redirectUrl页面。
             * 如果用户同意授权，页面将跳转至 redirect_uri/?code=CODE&state=STATE。这里的code用于换取access_token（和通用接口的access_token不通用）
             * 若用户禁止授权，则重定向后不会带上code参数，仅会带上state参数redirect_uri?state=STATE
             */
            return url;
        }
        /// <summary>
        /// 这里通过code换取的是一个特殊的网页授权access_token,
        /// 与基础支持中的access_token（该access_token用于调用其他接口）不同。
        /// 公众号可通过下述接口来获取网页授权access_token。
        /// 如果网页授权的作用域为snsapi_base，则本步骤中获取到网页授权access_token的同时，
        /// 也获取到了openid，snsapi_base式的网页授权流程即到此为止。
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public Oauth2_access_token GetOpenId(string code)
        {
            var appId = _api.GetAppid();
            var secret = _api.GetSecret();

            var url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code",
                                   appId, secret, code);

            return Get<Oauth2_access_token>(url);
        }


        /// <summary>
        /// 刷新access_token（如果需要）
        /// </summary>
        /// <param name="refreshToken">填写通过access_token获取到的refresh_token参数</param>
        /// <param name="grantType"></param>
        /// <returns></returns>
        public OAuthAccessTokenResult RefreshToken(string refreshToken, string grantType = "refresh_token")
        {
            var appId = _api.GetAppid();
            var url = string.Format("https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type={1}&refresh_token={2}",
                                appId, grantType, refreshToken);
            return Get<OAuthAccessTokenResult>(url);
        }

        /// <summary>
        /// 获取用户基本信息
        /// </summary>
        /// <param name="openId">普通用户的标识，对当前公众号唯一</param>
        /// <param name="lang">返回国家地区语言版本，zh_CN 简体，zh_TW 繁体，en 英语</param>
        /// <returns></returns>
        public  OAuthUserInfo GetUserInfo( string openId, string lang = "zh_CN")
        {
            var accessToken = _api.GetAccessToken();
            var url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}&lang={2}", accessToken, openId, lang);
            return Get<OAuthUserInfo>(url);
        }

        /// <summary>
		/// 检验授权凭证（access_token）是否有效
		/// </summary>
        /// <param name="openId">用户的唯一标识</param>
		/// <returns></returns>
		public  JsonResult Auth( string openId)
        {
            var accessToken = _api.GetAccessToken();
            var url = string.Format("https://api.weixin.qq.com/sns/auth?access_token={0}&openid={1}", accessToken, openId);
            return Get<JsonResult>(url);
        }
    }
}
namespace TG.Common.Weixin
{
    partial class WxMpApi
    {
        private OAuthApi _OAuthApi;
        /// <summary>
        /// 自定义菜单管理
        /// </summary>
        public OAuthApi OAuthApi
        {
            get
            {
                if (_OAuthApi==null)
                {
                    _OAuthApi= new OAuthApi(this);
                }
                return _OAuthApi;
            }
        }

    }
}