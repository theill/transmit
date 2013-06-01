
var appletAlreadyInitialized = false;

// fix for IE9 not calling liveconnect events at all
function appletInitializedProxy() {
	if (!appletAlreadyInitialized) {
		appletInitialized(document.jumpLoaderApplet);
	}
}

function appletInitialized(applet) {
	//console.debug("appletInitialized");

	appletAlreadyInitialized = true;

	var uploader = applet.getUploader();
	var attrSet = uploader.getAttributeSet();
	var attr = attrSet.createStringAttribute("token", GetToken());
	attr.setSendToServer(true);

	var config = applet.getUploaderConfig();
	config.setUserAgent(navigator.userAgent);
	config.setUploadUrl(TransmitSettings.UploadUrl);
	config.setMaxFileLength(TransmitSettings.MaxFileSizeKb * 1024);

	// fix start/stop button visibility bug in jumploader for Firefox on MacOSX
	var viewConfig = applet.getViewConfig();
	viewConfig.setUploadViewStartActionVisible(false);
	viewConfig.setUploadViewStopActionVisible(false);
	setTimeout("document.jumpLoaderApplet.getMainView().getUploadView().updateView()", 1000);

//		$("#uploadApplet div").attr("style", "width: 620px; height: 50px; overflow: hidden");
	$("#uploadApplet").addClass("loaded");
}

function getUploader() {
	return document.jumpLoaderApplet.getUploader();
}

function startUpload(index) {
	if (getUploader().getStatus() == 1) {
		// stop currently active upload
		stopUpload(index);
		return;
	}

	var error = getUploader().startUpload();
	if (error != null) {
		alert("It was not possible to start your upload at the moment. Please try again later.\n\nTechnical error: " + error);
		return;
	}
		
	setTimeout(checkFile, 1000);
}

function stopUpload(index) {
	var error = getUploader().stopUpload();
	if (error != null) {
		alert(error);
		return;
	}
}

//	function uploaderFilesReset(uploader) {
//		console.debug("uploaderFilesReset");
//	}
 
//	function uploaderFileAdded(uploader, file) {
//		console.debug("uploaderFileAdded(uploader=" + uploader + ", file=" + file + ")");
//	}

//	function uploaderFileRemoved(uploader, file) {
//		console.debug("uploaderFileRemoved");
//	}

//	function uploaderFileRemoved(uploader, file, oldIndex) {
//		console.debug("uploaderFileRemovedx");
//	}

//	function uploaderFileStatusChanged(uploader, file) {
	//console.debug("uploaderFileStatusChanged for " + file.getName() + " to status " + file.getStatus());

	/*
027            public static final int STATUS_READY = 0;
028            public static final int STATUS_UPLOADING = 1;
029            public static final int STATUS_FINISHED = 2;
030            public static final int STATUS_FAILED = 3;
031            public static final int STATUS_PREPROCESSING = 4;
032            public static final int STATUS_DOWNLOADING = 5;
033            public static final int STATUS_EDITING = 6;
	*/
		
//		if (file.getStatus() == 1) { // uploading
//			var tp = file.getTransferProgress();
//			//console.debug("percentage done: " + tp.getCompletionPercent());
//		}
//	}

//	function uploaderStatusChanged(uploader) {
	//console.debug("uploaderStatusChanged");
//	}

function checkFile() {
	if (getUploader().getStatus() == 0) {
		// only run when status is STATUS_UPLOADING (i.e. 1)
		return;
	}

	//console.debug("checking files...");
	var fileCount = getUploader().getFileCount();
	for (var i = 0; i < fileCount; i++) {
		var file = getUploader().getFile(i.toString());
		if (file.getStatus() == 1) { // uploading
			var tp = file.getTransferProgress();
			//console.debug("percentage done: " + tp.getCompletionPercent());
		}
	}
	setTimeout(checkFile, 1000);
}

$(function() {
//		// http://bugs.sun.com/view_bug.do?bug_id=4774374
//		$("#uploadApplet div").attr("style", "width: 1px; height: 1px; overflow: hidden");

	setTimeout("appletInitializedProxy()", 5000);
});

