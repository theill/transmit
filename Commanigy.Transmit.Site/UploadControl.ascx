<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="UploadControl.ascx.cs" Inherits="Commanigy.Transmit.Site.UploadControl" %>
<% if (Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.UploadChunked) { %>
<script type="text/javascript" src="<%= Commanigy.Transmit.Data.TransmitSettings.Instance.Setting.ExternalUrl %>/js/upload-applet.js"></script>

<ul id="uploader-queue">
	<li class="no-files">
		<p><%= _("Drop files into area below to queue them for upload.") %><br /><%= _("Alternatively double-click the area to open a \"Browse\" dialog to locate files on your local hard drive.") %></p>
	</li>
</ul>
<div id="uploadApplet">
<applet name="jumpLoaderApplet"
	code="jmaster.jumploader.app.JumpLoaderApplet.class"
	archive="<%= Commanigy.Transmit.Data.TransmitSettings.Instance.UploadAppletArchiveUrl %>"
	width="620"
	height="50"
	mayscript="mayscript">
		<param name="mayscript" value="true" />
		<param name="scriptable" value="true" />

		<param name="uc_partitionLength" value="1048576" />
		<param name="uc_directoriesEnabled" value="true" />
		<param name="vc_mainViewShowUploadErrors" value="true" />
        <param name="vc_useThumbs" value="false" />
		<param name="vc_lookAndFeel" value="system" />
		<param name="vc_mainViewLogoEnabled" value="false" />
		<param name="vc_uploadViewUseThumbs" value="false" />
		<param name="vc_uploadViewMenuBarVisible" value="false" />
		<param name="vc_uploadViewProgressPaneVisible" value="false" />
		<param name="vc_uploadViewStartActionVisible" value="false" />
		<param name="vc_uploadViewStopActionVisible" value="false" />
		<param name="vc_uploadViewListShowFileSize" value="true" />
		<param name="vc_uploadViewListStatusVisible" value="false" />
		<param name="vc_uploadViewAutoscrollToUploadingFile" value="true" />
		<param name="vc_uploadListViewName" value="_compact" />

		<param name="ac_fireAppletInitialized" value="true" />
		<param name="ac_fireUploaderFileAdded" value="false" />
		<param name="ac_fireUploaderFileMoved" value="false" />
		<param name="ac_fireUploaderFileRemoved" value="false" />
		<param name="ac_fireUploaderFilesReset" value="false" />
		<param name="ac_fireUploaderFileStatusChanged" value="true" />
		<param name="ac_fireUploaderSelectionChanged" value="false" />
		<param name="ac_fireUploaderStatusChanged" value="false" />
</applet>
</div>

<% } else { %>
<script type="text/javascript">

	$(function() {
		if (!$.fileUploadSupported) {
			$("#dropbox").hide();
		} else {
			$("#dropbox").show();
		}
	
		// Enable plug-in
		$('#dropbox').fileUpload({
			url: '<%= Commanigy.Transmit.Data.TransmitSettings.Instance.UploadUrl %>',
			type: 'POST',
			dataType: 'text',
			beforeSend: function () {
				$(document.body).addClass('uploading');

				$("#uploader-queue .no-files").hide();
				$("#uploader-queue").removeClass("warning");

				var queueID = 'uploader-queue';

				jQuery("#uploader-queue").append('<li id="FuUploads' + queueID + '" queueID="' + queueID + '" class="queue-item queued">\
							<a class="file-cancel" href="#" title=\"" + Transmit.t("Cancel") + "\">" + Transmit.t("Cancel") + "</a>\
							<span class="file-name">' + Transmit.ellipse('dragged file', 40) + '</span>\
							<span class="file-type">' + 'some type' + '</span>\
							<span class="file-size">' + Transmit.formatSize(0) + '</span>\
							<div class="progressbar">&nbsp;</div>\
						</li>');

				console.log("before send");
			},
			complete: function () {
				console.log("complete");
				$(document.body).removeClass('uploading');
			},
			success: function (result, status, xhr) {
				console.log("success");
				if (!result) {
					window.alert('Server error.');
					return;
				}

				var queueID = 'uploader-queue';

				$("#FuUploads" + queueID + " .file-cancel").remove();
				$("#FuUploads" + queueID + " .progressbar").remove();

				if ($("input[name='Files[]'][value='" + result + "']").length == 0) {
					$("#FuUploads" + queueID)
						.removeClass("loading")
								.addClass("complete")
								.prepend("<a class=\"file-remove\" href=\"#\" title=\"" + Transmit.t("Remove") + "\">" + Transmit.t("Remove") + "</a>")
								.append("<span class=\"file-status\">" + Transmit.t("File was uploaded and ready for use") + "</span>")
								.append("<input type='hidden' name='Files[]' value='" + result + "' />");

					$("#FuUploads" + queueID + " .file-status").fadeOut(10000, function() {
						$(this).remove();
					});
				}
			}
		});
	
		$("#dropbox").bind(
			'dragenter',
			function () {
				$(this).addClass('dragging');
			}
		).bind(
			'dragleave',
			function () {
				$(this).removeClass('dragging');
			}
		);
	});

</script>

<div id="dropbox">
	<ul id="uploader-queue">
		<li class="no-files">
			<p><%= _("Click \"Add files...\" to select files you want to upload. Select multiple files by holding down Shift or Ctrl while clicking on the filename.")%></p>
		</li>
	</ul>
	<div class="action-bar">
		<ul>
			<li class="browse-files">
				<div><input type="file" name="FuUploads" id="FuUploads" /></div>
				<a id="uploader-browse" href="#"><%= _("Add files...")%></a>
			</li>
			<li class="upload-files"><a id="uploader-upload" href="#"><%= _("Upload queued files")%></a></li>
			<li class="clear-files"><a id="uploader-clear" href="#"><%= _("Clear all")%></a></li>
			<li id="progress"></li>
		</ul>
		<div class="clear"></div>
	</div>
</div>
<% } %>