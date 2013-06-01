String.prototype.trim = function() { return this.replace(/^\s+|\s+$/, ''); };
String.prototype.safeFileName = function() { return this.replace(/,/, '-'); };

Transmit = {
  uploading: false,
  t: function(k) {
    return TransmitMessages[k] || k;
  },
  join: function(a, b) {
    if (a == "") {
      return b;
    }
    else if (b == "") {
      return a;
    }

    return a + ", " + b;
  },
  ellipse: function(s, len) {
    return (s.length > len) ? s.substr(0, 40) + '...' : s;
  },
  showError: function(msg) {
    $(".flash").html("<div class=\"flash-error\">" + msg + "</div>");
    $('html, body').animate({ scrollTop: 0 });
  },
  toggleHelp: function() {
    if ($("#help").length == 0) {
      return true;
    }

    if ($("#help").is(':visible')) {
      $("#help").hide();
    }
    else {
      $("#help").fadeIn()
      var h2 = $("#help").find("h2");
      if (h2.find("a#close-help").length == 0) {
        h2.append("<a href=\"#\" id=\"close-help\" title=\"" + Transmit.t("Close") + "\">" + Transmit.t("Close") + "</a>");
      }
    }

    return false;
  },
  formatSize: function(fileSizeInBytes) {
    var byteSize = Math.round(fileSizeInBytes / 1024 * 100) * .01;
    var suffix = 'KB';
    if (byteSize > 1000) {
      byteSize = Math.round(byteSize * .001 * 100) * .01;
      suffix = 'MB';
    }
    var sizeParts = byteSize.toString().split('.');
    if (sizeParts.length > 1) {
      decimals = sizeParts[1].substr(0, 2);
      byteSize = sizeParts[0] + '.' + (decimals.length == 1 ? '0' + decimals : decimals);
    } else {
      byteSize = sizeParts[0];
    }
    return byteSize + suffix;
  },
  isValidEmail: function(email) {
    return /^([a-zA-Z0-9_.-])+@([a-zA-Z0-9_.-])+\.([a-zA-Z])+([a-zA-Z])+/.test(email);
  },
  addRecipient: function(item) {
    //        $("#recipients").prepend("<li><a class=\"recipient-remove\" href=\"#\" title=\"Remove this recipient\">Remove</a><span class=\"display-name\">" + item.displayName + "</span><span class=\"details\">" + (item.type == "contact" ? "<span class=\"mail\">" + item.mail + "</span> " : "") + (item.title ? "<span class=\"title\">" + item.title + "</span>, " : "") + "<span class=\"department\">" + (item.department || '&nbsp;') + "</span></span><input type='hidden' name='Recipients[]' value='" + item.mail + "' /></li>");
    $("#recipients").prepend("<li><a class=\"recipient-remove\" href=\"#\" title=\"" + Transmit.t("Remove as recipient") + "\">" + Transmit.t("Remove") + "</a>" + Transmit.formatRecipient(item) + "<input type='hidden' name='Recipients[]' value='" + item.mail + "' /></li>");
    $("#recipients li:first").effect("highlight", {}, 500);
  },
  formatRecipient: function(item) {
    var pictureTag = "";
    if (item.url && item.url.match(/\.(jpg|png|gif|bmp)$/i)) {
        pictureTag = "<img src=\"" + TransmitSettings.PictureUrl + "?u=" + encodeURIComponent(item.url) + "\" border=\"0\" />";
    }
    else if (item.type == "Group") {
        pictureTag = "<img src=\"images/group.png\" width=\"32\" height=\"32\" />";
    }
    else if (item.type == "User") {
        pictureTag = "<img src=\"images/no-user-picture.png\" width=\"32\" height=\"32\" />";
    }
    else {
        pictureTag = "";
    }

    return "<div class=\"user-picture\">" + pictureTag + "</div><div class=\"" + item.type.toLowerCase() + "\"><span class=\"display-name\"><a href=\"profile.aspx?id=" + item.mail + "\" title=\"Go to " + item.displayName + "'s public profile\" target=\"_blank\">" + item.displayName + "</a></span>" + ((item.displayName != item.mail) ? "<span class=\"email-address\">" + item.mail + "</span>" : "") + "<span class=\"title\">" + (item.title || '&nbsp;') + "</span><div class=\"details\">" + Transmit.join((item.department ? "<span class=\"department\">" + item.department + "</span>" : ""), (item.location ? "<span class=\"location\">" + item.location + "</span>" : "")) + (item.country ? " (<span class=\"country\">" + item.country + "</span>)" : "") + "</div></div>";
  },
  isUploading: function() {
    return this.uploading;
  },
  packUploadedFiles: function() {
    // only pack files when using jump loader
    if (document.jumpLoaderApplet == undefined) {
      return;
    }
    
    // clear all files in array (if any)
    var allFiles = $("input[name='Files[]']");
    $.each(allFiles, function(f) {
      $(f).remove();
    });

    var uploader = document.jumpLoaderApplet.getUploader();
    
    for (var i = 0; i < uploader.getFileCount(); i++) {
      var file = uploader.getFile(i.toString());
      if (file.getStatus() == 2) { // finished
        $("#aspnetForm").append("<input type='hidden' name='Files[]' value='" + file.getName().safeFileName() + "' />");
      }
    }
  },
  startUploading: function() {
    this.uploading = true;
    this.checkUploadingStatus();
  },
  finishUploading: function() {
    this.uploading = false;
    this.packUploadedFiles();
    $(".submit-button").attr("data-uploaded", "true");
    __doPostBack("ctl00$MainContent$LnkShare",'');
//    $(".submit-button").click();
  },
  checkUploadingStatus: function() {
    var uploader = document.jumpLoaderApplet.getUploader();

    if (!uploader.isUploading()) {
      uploader.startUpload();
    }

    var completed = true;
	var fileCount = uploader.getFileCount();
	for (var i = 0; i < fileCount; i++) {
	  var file = uploader.getFile(i.toString());
	  if (file.getStatus() == 1) {
        completed = false;
	  }
	}

    if (completed && uploader.getStatus() == 0) {
      // everything complete
      Transmit.finishUploading();
    }
    else {
      setTimeout(Transmit.checkUploadingStatus, 1000);
    }
  },
  fileCount: function() {
    return (document.jumpLoaderApplet == undefined) ? $("input[name='Files[]']").length : document.jumpLoaderApplet.getUploader().getFileCount();
  },
  keepPageAlive: function() {
    $.poll(30000, function(retry) {
      $.post('KeepSessionAlive.ashx', null, function() { });
      retry();
    });
  }
};

; (function($) {
  $.poll = function(ms, callback) {
    if ($.isFunction(ms)) {
      callback = ms
      ms = 1000
    }
    (function retry() {
      setTimeout(function() {
        callback(retry)
      }, ms)
      //      ms *= 1.5
    })()
  }
})(jQuery);

//        'scriptData': { ".ASPXAUTH": '', "ASPSESSID": '' },
$(document).ready(function() {

  $("#n li").bind("click", function(e) {
//    var href = $(this).find("a").attr("href");
//    $(this).find("a").trigger("click");
//    if (href) {
//      window.location.href = href;
//      return true;
//    }
//
//    return false;
  });

  $(".button").hover(
    function() {
      $(this).addClass("hover");
    },
    function() {
      $(this).removeClass("hover");
    }
  );

  var formSubmitted = null;
  var allCompleted = true;

  var failedUploaded = [];

  $("#FuUploads").uploadify({
    'uploader': 'js/uploadify.swf',
    'script': TransmitSettings.UploadUrl,
    'method': 'POST',
    'expressInstall': 'js/expressInstall.swf',
    'buttonImg': 'images/transparent.gif',
    'wmode': 'transparent',
    'width': 134,
    'height': 24,
    'simUploadLimit': 1,
    'queueID': 'uploader-queue',
    'auto': true,
    'multi': true,
    'scriptAccess': 'always',
    'sizeLimit': (TransmitSettings.MaxFileSizeKb * 1024),
    'fileDataName': 'payload',
    onSelect: function(evt, queueID, fileObj) {
      $("#uploader-queue .no-files").hide();
      $("#uploader-queue").removeClass("warning");

      queue = '#FuUploadsQueue';
      if (evt.data.queueID) {
        queue = '#' + evt.data.queueID;
      }
      jQuery(queue).append('<li id="FuUploads' + queueID + '" queueID="' + queueID + '" class="queue-item queued">\
					<a class="file-cancel" href="#" title=\"" + Transmit.t("Cancel") + "\">" + Transmit.t("Cancel") + "</a>\
					<span class="file-name">' + Transmit.ellipse(fileObj.name, 40) + '</span>\
					<span class="file-type">' + fileObj.type + '</span>\
					<span class="file-size">' + Transmit.formatSize(fileObj.size) + '</span>\
					<div class="progressbar">&nbsp;</div>\
				</li>');

      $("#uploader-browse").html("Add more files...");
      return false;
    },
    onSelectOnce: function(evt, data) {
      if (typeof(GetToken) == 'function') {
        $("#FuUploads").uploadifySettings("scriptData", { "token": GetToken() });
      }
      return false;
    },
    onComplete: function(evt, queueID, fileObj, response, data) {
      $("#FuUploads" + queueID + " .file-cancel").remove();
      $("#FuUploads" + queueID + " .progressbar").remove();

      if ($("input[name='Files[]'][value='" + fileObj.name.safeFileName() + "']").length == 0) {
        $("#FuUploads" + queueID)
          .removeClass("loading")
				  .addClass("complete")
				  .prepend("<a class=\"file-remove\" href=\"#\" title=\"" + Transmit.t("Remove") + "\">" + Transmit.t("Remove") + "</a>")
				  .append("<span class=\"file-status\">" + Transmit.t("File was uploaded and ready for use") + "</span>")
				  .append("<input type='hidden' name='Files[]' value='" + response.safeFileName() + "' />");

        $("#FuUploads" + queueID + " .file-status").fadeOut(10000, function() {
          $(this).remove();
        });
      }
      else {
        $("#FuUploads" + queueID)
          .removeClass("loading")
          .addClass("error")
          .prepend("<a class=\"file-error\" href=\"#\" title=\"" + Transmit.t("Clear") + "\">" + Transmit.t("Clear") + "</a>")
          .append("<span class=\"file-status\">" + Transmit.t("File was already uploaded and has been ignored") + "</span>");
      }

      return false;
    },
    onAllComplete: function(evt, data) {
      $("#progress").html("");

      $.each(failedUploaded, function(idx, queueID) {
        if ($("#FuUploads" + queueID).length == 1) {
          $("#FuUploads").uploadifyCancel(queueID);
        }
      });

      failedUploaded = [];

      allCompleted = true;

      // submit form if we have a pending submit
      if (formSubmitted) {
        formSubmitted.click();
      }

      return false;
    },
    onCancel: function(evt, queueID, fileObj) {
      $("#progress").html("");
      return false;
    },
    onError: function(evt, queueID, fileObj, errorObj) {
      var errorMessage = Transmit.t("Failed to upload file.") + " " + errorObj.type + " (" + errorObj.info + ")";
      if (errorObj.type == "File Size") {
        errorMessage = Transmit.t("File is too big");
        // failed uploads should be removed when all uploads are completed
        failedUploaded.push(queueID);
      }
      else if (errorObj.type == "IO") {
        errorMessage = Transmit.t("File could not be stored on remote server.") + " " + errorObj.info;
        // failed uploads should be removed when all uploads are completed
        failedUploaded.push(queueID);
      }

      $("#FuUploads" + queueID).find(".file-cancel").remove();
      $("#FuUploads" + queueID + " .progressbar").remove();
      $("#FuUploads" + queueID)
        .removeClass("loading")
        .addClass("error")
        .prepend("<a class=\"file-error\" href=\"#\" title=\"" + Transmit.t("Clear") + "\">" + Transmit.t("Clear") + "</a>")
        .append("<span class=\"file-status\">" + errorMessage + "</span>");

      return false;
    },
    onProgress: function(evt, queueID, fileObj, data) {
      allCompleted = false;

      //$("#progress").html("Uploaded (at " + Transmit.formatSize(data.speed * 1024) + "/s) " + Transmit.formatSize(data.allBytesLoaded));
      $("#progress").html("Uploaded " + data.percentage + "%");
      $("#FuUploads" + queueID + " .file-size").html(Transmit.formatSize(data.bytesLoaded));
      // data.bytesLoaded, data.percentage, data.allBytesLoaded, data.speed
      if (data.percentage == 100) {
        $("#progress").html(Transmit.t("Scanning and compressing file..."));
      }

      $("#FuUploads" + queueID).removeClass("queued").addClass("loading");
      $("#FuUploads" + queueID + " .progressbar").css("width", ((618 - 2 - 24) / (100 / data.percentage)) + 'px');

      var currentUploadPosition = $("#FuUploads" + queueID).attr("offsetTop")
      if ((currentUploadPosition + 25) > 100) {
        // we need to scroll in order for this to scroll properly into view
        $("#uploader-queue").scrollTop(currentUploadPosition);
      }

      return false;
    }
  });

  $("#uploader-clear").live('click', function() {
    $("#FuUploads").uploadifyClearQueue();
    $("#uploader-queue .queue-item").each(function() {
      $(this).fadeOut(250, function() {
        $(this).remove();
      });
    });
    //        $("#uploader-queue .no-files").animate({ opacity: 1.0 }, 250, "linear", function() { $(this).fadeIn('slow'); });
    return false;
  });

  if (swfobject.getFlashPlayerVersion().major == 0) {
    $(".action-bar").html(Transmit.t("You need Flash to upload files"));
  }

  $("#uploader-queue .queue-item .file-cancel").live("click", function() {
    var item = $(this).parent("li");
    $("#FuUploads").uploadifyCancel(item.attr("queueID"));
    item.fadeOut(250, function() {
      item.remove();
    });
    if ($("#uploader-queue .queue-item").length <= 1) {
      $("#uploader-queue .no-files").animate({ opacity: 1.0 }, 250, "linear", function() { $(this).fadeIn('slow'); });
    }
    return false;
  });

  $("#uploader-queue .queue-item .file-remove, #uploader-queue .queue-item .file-error").live("click", function() {
    var item = $(this).parent("li");
    item.fadeOut(250, function() {
      item.remove();
    });
    if ($("#uploader-queue .queue-item").length <= 1) {
      $("#uploader-queue .no-files").animate({ opacity: 1.0 }, 250, "linear", function() { $(this).fadeIn('slow'); });
      $("#uploader-browse").html(Transmit.t("Add files..."));
    }
    return false;
  });

  $("#uploader-browse").live('click', function() {
    //$("#FuUploads").uploadifyBrowse();
    //return false;
  });

  $("#uploader-upload").live('click', function() {
    $("#FuUploads").uploadifyUpload();
    return false;
  });

//  $(".upload-with-submit").live("click", function() {
//    if (!allCompleted && !formSubmitted) {
//      formSubmitted = this;
//      return false;
//    }
//    return true;
//  });

  $("#faq li a").live("click", function() {
    $(this).parent().find(".faq-item").toggle();
    return false;
  });

  $(".toggle-help a").live("click", function() {
    return Transmit.toggleHelp();
  });

  $("#close-help").live("click", function() {
    return Transmit.toggleHelp();
  });

  $("#display-language-picker").live("click", function() {
    //$("#current-language").hide();
    $("#current-language").animate({ opacity: 0 }, {duration: 500, queue: false});
    $("#current-language").hide('slide', {direction: 'right'}, 500);
    $("#language-picker").delay(500).show('slide', {direction: 'right'}, 250);
    return false;
  });

  $("#recipients .recipient-remove").live("click", function() {
    var item = $(this).parent("li");
    item.fadeOut(250, function() {
      item.remove();
    });
    //		if ($("#recipients li").length <= 1) {
    //			$("#recipients .no-files").animate({opacity: 1.0}, 250, "linear", function() { $(this).fadeIn('slow'); });
    //		}
    return false;
  });
});