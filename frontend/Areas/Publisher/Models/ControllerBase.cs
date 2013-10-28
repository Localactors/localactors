using System;
using System.Collections.Generic;
using System.Configuration;
using System.Drawing;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Localactors.entities;
using Amazon.S3;
using Amazon.S3.Model;
using NLog;
using System.Linq;

namespace Localactors.webapp.Areas.Publisher
{
    [HandleError]
    public class ControllerBase : Localactors.webapp.ControllerBase {


    }
}
