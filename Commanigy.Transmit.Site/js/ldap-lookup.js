$(document).ready(function() {

    $("#TbxRecipients").autocomplete(TransmitSettings.UsersLookupUrl, {
        dataType: 'jsonp',
        parse: function(data) {
            var rows = new Array();
            rows[0] = { data: { "displayName": "", "mail": "", "department": "", "total": data.length.toString() }, value: "" };
            for (var i = 0; i < Math.min(data.length, 25); i++) {
                rows[i + 1] = { data: data[i], value: data[i].mail, result: "" };
            }
            return rows;
        },
        max: 25,
        scroll: true,
        scrollHeight: 190,
        cacheLength: 0,
        formatItem: function(item, idx, total) {
            if (idx > 1) {
                return "<div class=\"addable\">" + Transmit.formatRecipient(item) + "</div>";
            }
            else {
                email = $("#TbxRecipients").val();
                if (Transmit.isValidEmail(email)) {
                    return "<div class=\"addable\"><div class=\"only-email\">Add <b>" + $("#TbxRecipients").val() + "</b> as an email address?</div></div>";
                }
                else {
                    if (total > 1) {
                        var moreFound = "";
                        if (total < item.total) {
                            moreFound = "Showing only " + total + " of " + item.total + " results";
                        }
                        return "<div class=\"no-user\">Select a recipient below or enter full e-mail address" + (moreFound != "" ? ". <span class=\"more-results\">" + moreFound + "</span>" : "") + "</div>";
                    }
                    else {
                        return "<div class=\"no-user\">No recipients matching your criteria</div>";
                    }
                }
            }
        }
    }).result(function(e, item) {
        if (item.mail == "") {
            // first element selected
            var enteredValue = $("#TbxRecipients").val();
            if (!Transmit.isValidEmail(enteredValue)) {
                return false;
            }
            
            // support using ',' and ';' for specifying multiple email addresses
            jQuery(enteredValue.split(",").join(";").split(";")).each(function(idx, email) {
                email = email.trim();
                if (Transmit.isValidEmail(email)) {
                    var item = { 'mail': email, 'displayName': email, 'type': 'Mail' };
                    Transmit.addRecipient(item);
                }
            });

            $("#TbxRecipients").val("");
            return false;
        }

        Transmit.addRecipient(item);
    });

    $("#TbxRecipients").bind("blur", function() {
      email = $(this).val();
      if (Transmit.isValidEmail(email)) {
        // automatically add it to list of recipients
        var item = { 'mail': email, 'displayName': email, 'type': 'Mail' };
        Transmit.addRecipient(item);
        $(this).val('');
      }
    });
});