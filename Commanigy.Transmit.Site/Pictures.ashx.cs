#region Using directives
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using log4net;
using Commanigy.Transmit.Web;
using Commanigy.Transmit.Data;
#endregion

namespace Commanigy.Transmit.Site {
	/// <summary>
	/// TODO: consider adding a cache layer to avoid requesting and resizing images all the time.
	/// </summary>
	[WebService(Namespace = "http://commanigy.com/")]
	[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
	public class Pictures : IHttpHandler {
		private ILog log = LogManager.GetLogger("site");

		/// <summary>
		/// Supports 'm' for mail, 'u' for actual url and 'd' for dimension (defaults: 32).
		/// Use 'p' to specify url for a default picture in case user wasn't found.
		/// </summary>
		/// <param name="context"></param>
		public void ProcessRequest(HttpContext context) {
			int dimension = 32;

			string imageUrl = string.Empty;

			string mail = context.Request["m"];
			if (!string.IsNullOrEmpty(mail)) {
				AuthenticatedUser user = Locator.FindByMail(mail);
				if (user != null) {
					imageUrl = user.Url;
				}
			}

			if (string.IsNullOrEmpty(imageUrl)) {
				imageUrl = HttpUtility.UrlDecode(context.Request["u"]);
			}

			if (!string.IsNullOrEmpty(context.Request["d"])) {
				dimension = Convert.ToInt32(context.Request["d"]);
			}

			if (string.IsNullOrEmpty(imageUrl) && !string.IsNullOrEmpty(context.Request["p"])) {
				imageUrl = HttpContext.Current.Server.MapPath(context.Request["p"]);
			}

			if (string.IsNullOrEmpty(imageUrl)) {
				log.Error("Called pictures handler but was not able to find any image url");
				imageUrl = HttpContext.Current.Server.MapPath("images/no-user-picture.png");
			}

			Bitmap image = GetImage(imageUrl);
			if (image == null) {
				imageUrl = HttpContext.Current.Server.MapPath("images/no-user-picture.png");
				image = Bitmap.FromFile(imageUrl) as Bitmap;
			}

			context.Response.Clear();
			context.Response.ContentType = GetContentType(image.RawFormat);

			HttpCachePolicy cache = context.Response.Cache;
			cache.SetAllowResponseInBrowserHistory(true);
			cache.SetCacheability(HttpCacheability.Public);
			cache.SetMaxAge(new TimeSpan(30, 0, 0, 0));
			cache.SetExpires(DateTime.Now.AddDays(7));
			cache.SetLastModifiedFromFileDependencies();
			cache.SetValidUntilExpires(true);

			byte[] buffer = ResizeImage(image, dimension);
			context.Response.OutputStream.Write(buffer, 0, buffer.Length);
			context.Response.End();
		}

		private Bitmap GetImage(string url) {
			if (!url.ToLower().StartsWith("http://") && !url.ToLower().StartsWith("https://") && !System.IO.File.Exists(url)) {
				url = TransmitSettings.Instance.Setting.ExternalUrl + (url.StartsWith("/") ? url : "/" + url);
				log.DebugFormat("Rewriting image url to {0}", url);
			}

			if (!Uri.IsWellFormedUriString(url, UriKind.RelativeOrAbsolute)) {
				try {
					return Bitmap.FromFile(url) as Bitmap;
				}
				catch (Exception x) {
					log.WarnFormat("Unable to get image from {0} which was assumed a local path. Cause: {1}", url, x.Message);
					return null;
				}
			}

			try {
				WebRequest imageRequest = WebRequest.Create(url);
				imageRequest.PreAuthenticate = true;
				imageRequest.UseDefaultCredentials = true;
				WebResponse imageResponse = imageRequest.GetResponse();
				return Bitmap.FromStream(imageResponse.GetResponseStream()) as Bitmap;
			}
			catch (Exception x) {
				log.WarnFormat("Not able to request image from {0}. Cause: {1}", url, x.Message);
				return null;
			}
		}

		/// <summary>
		/// Resizes an image to a minimum dimension. If image is vertically 
		/// larger it will be resized to a minimum height of 'dimension' and
		/// the other way around (horizontally larger images will be resized
		/// to a minimum width of 'dimension'.
		/// </summary>
		/// <param name="fullSizeImage"></param>
		/// <param name="dimension">Minimum dimension (width or height) of image</param>
		/// <returns></returns>
		private byte[] ResizeImage(Image fullSizeImage, int dimension) {
			// store original image format (used when streaming thumbnail back)
			ImageFormat imageFormat = fullSizeImage.RawFormat;

			// trick - prevent using images internal thumbnail
			fullSizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);
			fullSizeImage.RotateFlip(RotateFlipType.Rotate180FlipNone);

			Image thumbnail;

			if (fullSizeImage.Height > fullSizeImage.Width) {
				int calculatedHeight = fullSizeImage.Height * dimension / fullSizeImage.Width;

				// resize image to new size (width is static)
				thumbnail = fullSizeImage.GetThumbnailImage(dimension, calculatedHeight, null, IntPtr.Zero);
			}
			else {
				int calculatedWidth = fullSizeImage.Width * dimension / fullSizeImage.Height;

				// resize image to new size (height is static)
				thumbnail = fullSizeImage.GetThumbnailImage(calculatedWidth, dimension, null, IntPtr.Zero);
			}

			// return binary stream
			using (MemoryStream imageStream = new MemoryStream()) {
				thumbnail.Save(imageStream, imageFormat);
				return imageStream.ToArray();
			}
		}

		private string GetContentType(ImageFormat format) {
			if (format == ImageFormat.Jpeg) {
				return "image/jpeg";
			}
			else if (format == ImageFormat.Png) {
				return "image/png";
			}
			else if (format == ImageFormat.Gif) {
				return "image/gif";
			}
			else if (format == ImageFormat.Bmp) {
				return "image/bmp";
			}

			return "image/jpeg";
		}

		public bool IsReusable {
			get {
				return true;
			}
		}
	}
}
