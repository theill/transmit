#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using System.Web;
using System.Web.Caching;
using System.Threading;
using Commanigy.Transmit.Data;
using System.Security.Cryptography; 
#endregion

namespace Commanigy.Transmit.Web {
	public class Localizer {
		private static ILog log = LogManager.GetLogger("web");

		private static MD5CryptoServiceProvider crypto = new System.Security.Cryptography.MD5CryptoServiceProvider();

		public static string t(string v, params object[] a) {
			string key = Thread.CurrentThread.CurrentCulture.ToString() + " " + HashString(v);
			
			Dictionary<string, string> messages = HttpContext.Current.Cache["Transmit.Messages"] as Dictionary<string, string>;
			if (messages == null) {
				log.InfoFormat("Reading localized messages");
				messages = new Dictionary<string, string>();

				using (DataClassesDataContext db = new DataClassesDataContext()) {
					var localizedMessages = from msg in db.Messages
											select msg;
					foreach (var m in localizedMessages) {
						messages.Add(string.Format("{0} {1}", m.Culture, m.LookupKey), m.LocalizedMessage);
					}
				}

				HttpContext.Current.Cache.Add("Transmit.Messages", messages, null, Cache.NoAbsoluteExpiration, new TimeSpan(4, 0, 0), CacheItemPriority.Default, null);
			}
			
			if (messages.ContainsKey(key)) {
				return string.Format(messages[key], a);
			}

			log.DebugFormat("No localization for {0} => {1}", key, v);
			StoreLocalizedMessage(key, v);

			return string.Format(v, a);

			//string filename = "c:\\temp\\localization.resources";
			//string key = v.ToLower().Replace(" ", "_").Replace("\"", "_").Replace(".", "_");

			//if (!new FileInfo(filename).Exists) {
			//    ResourceWriter writer = new ResourceWriter(filename);
			//    writer.AddResource(key, v);
			//    writer.Generate();
			//    writer.Close();
			//    log.Debug("Writing new resource file");
			//}

			//log.Debug("looking at key: " + key);

			//ResourceSet rs = new ResourceSet(filename);
			//object o = rs.GetObject(key);
			//log.Debug("got object = " + o);

			//string x = rs.GetString(key);
			//log.Debug("got string = " + o);
			//if (x != null) {
			//    v = x;
			//}
			////Console.WriteLine(Rs.GetDefaultReader().ToString());
			//rs.Close();

			//try {
			//    //string b = HttpContext.GetLocalResourceObject(HttpContext.Current.Request.Path, "test") as string;
			//    //if (b != null) {
			//    //    v = "OK! " + b;
			//    //}
			//}
			//catch (InvalidOperationException) {
			//}

//			return string.Format(v, a);
		}

		private static void StoreLocalizedMessage(string key, string v) {
			string[] keyHash = key.Split(' ');

			using (DataClassesDataContext db = new DataClassesDataContext()) {
				Message m = db.Messages.FirstOrDefault(a => a.LookupKey == keyHash[1] && a.Culture == keyHash[0]);
				if (m == null) {
					m = new Message {
						Culture = keyHash[0],
						LookupKey = keyHash[1],
						LookupMessage = v,
						LocalizedMessage = v, // default to standard lookup message
						CreatedAt = DateTime.UtcNow
					};

					db.Messages.InsertOnSubmit(m);
					db.SubmitChanges();
				}
			}
		}

		private static string HashString(string value) {
			byte[] data = System.Text.Encoding.ASCII.GetBytes(value);
			data = crypto.ComputeHash(data);
			StringBuilder buffer = new StringBuilder();
			for (int i = 0; i < data.Length; i++) {
				buffer.Append(data[i].ToString("x2").ToLower());
			}
			return buffer.ToString();
		}
	}
}