using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using SufeiUtil;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace ShortUrl
{
    public partial class Form1 : Form
    {
        RegistryKey regkey;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            regkey = Registry.CurrentUser.OpenSubKey(@"Software\ShortUrl");
            if (regkey != null)
            {
                var BaiduTokenValue = regkey.GetValue("BaiduTokenValue");
                if (BaiduTokenValue != null)
                {
                    textBox1.Text = BaiduTokenValue.ToString();
                    if (!string.IsNullOrEmpty(textBox1.Text))
                        checkBox1.Checked = true;
                }
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            regkey = Registry.CurrentUser.CreateSubKey(@"Software\ShortUrl");
            var flag = checkBox1.Checked;
            if (flag)
                regkey.SetValue("BaiduTokenValue", textBox1.Text.Trim());
            else
                regkey.SetValue("BaiduTokenValue", string.Empty);
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://dwz.cn/console/userinfo");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var type = "t.cn";
                if (radioButton1.Checked) type = "t.cn";
                else if (radioButton2.Checked) type = "url.cn";
                else if (radioButton3.Checked) type = "dwz.cn";
                var yUrl = textBox2.Text.Trim();
                var dUrl = textBox3.Text.Trim();
                var baiduToken = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(yUrl) || !yUrl.StartsWith("http")) return;
                string url = "", html = "", result = "";
                if (type == "t.cn")
                {
                    url = "http://api.t.sina.com.cn/short_url/shorten.json?source=3271760578&url_long=" + HttpUtility.UrlEncode(yUrl);
                    html = GetHttpContent(url);
                    if (html.StartsWith("["))
                    {
                        var json = JArray.Parse(html);
                        result = json.First["url_short"].ToString();
                    }
                }
                else if (type == "url.cn")
                {
                    url = "http://sa.sogou.com/gettiny?url=" + HttpUtility.UrlEncode(yUrl);
                    html = GetHttpContent(url);
                    result = html;
                }
                else if (type == "dwz.cn")
                {
                    if (string.IsNullOrEmpty(baiduToken))
                    {
                        MessageBox.Show("百度Token不可为空"); return;
                    }
                    else
                    {
                        url = "https://dwz.cn/admin/v2/create";
                        string postData = "{\"url\":\"" + yUrl + "\",\"TermOfValidity\":\"long-term\"}";
                        html = GetHttpContent(url, "post", postData, new Dictionary<string, string> { { "Token", baiduToken } });
                        var json = JObject.Parse(html);
                        string code = json["Code"].ToString();
                        if (code == "0")
                        {
                            string ShortUrl = json["ShortUrl"].ToString();
                            result = ShortUrl;
                        }
                        else
                        {
                            string ErrMsg = json["ErrMsg"].ToString();
                            result = ErrMsg;
                        }
                    }
                }
                textBox3.Text = result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// 发送HTTP请求并获取返回数据
        /// </summary>
        /// <param name="url">URL</param>
        /// <param name="method">HTTP请求类型，GET或者POST</param>
        /// <param name="postdata">POST请求数据</param>
        /// <param name="headers">HTTP请求头</param>
        /// <returns></returns>
        public string GetHttpContent(string url, string method = "get", string postdata = "", Dictionary<string, string> headers = null)
        {
            HttpHelper http = new HttpHelper();
            HttpItem item = new HttpItem()
            {
                URL = url,//URL     必需项
                Encoding = null,//编码格式（utf-8,gb2312,gbk）     可选项 默认类会自动识别
                Method = method,//URL     可选项 默认为Get
                Timeout = 100000,//连接超时时间     可选项默认为100000
                ReadWriteTimeout = 30000,//写入Post数据超时时间     可选项默认为30000
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/76.0.3809.132 Safari/537.36",//用户的浏览器类型，版本，操作系统     可选项有默认值
                Accept = "text/html, application/xhtml+xml, */*",//    可选项有默认值
                ContentType = "text/html",//返回类型    可选项有默认值
                Postdata = postdata,//Post数据     可选项GET时不需要写
            };
            if (headers != null)
            {
                foreach (var k in headers)
                {
                    item.Header.Add(k.Key, k.Value);
                }
            }
            //得到HTML代码
            HttpResult result = http.GetHtml(item);
            //返回的Html内容
            return result.Html;
        }

    }
}
