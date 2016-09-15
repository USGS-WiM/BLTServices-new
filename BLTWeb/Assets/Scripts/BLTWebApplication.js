

$(document).ready(function () {

    //Check for mobile
    var mobile = (/iphone|ipad|ipod|android|blackberry|mobile|mini|windows\sce|palm/i.test(navigator.userAgent.toLowerCase()));
    var airBrowser = (/adobeair/i.test(navigator.userAgent.toLowerCase()));
    var cssUrl = "../Assets/Styles/desktop.css";

    //Switch to mobile css if called from mobile browser	
    if (mobile) {
        cssUrl = "../Assets/Styles/mobile.css";
    }

    //If desktop Air or mobile browser load additional css
    if (airBrowser || mobile) {
        //Load appropriate css
        var link = $("<link>");
        link.attr({
            type: 'text/css',
            rel: 'stylesheet',
            href: cssUrl
        });
        $("head").append(link);
    }

});


function json2xml(o, rootTag) {
    var toXml = function (v, name, ind) {
        var xml = "";
        if (typeof (v) == "object") {
            var hasChild = false;
            for (var m in v) {
                if (m.charAt(0) == "@") {
                    xml += " " + m.substr(1) + "=\"" + v[m].toString() + "\"";
                } else {
                    hasChild = true;
                }
            }
            if (hasChild) {
                for (var m in v) {
                    if (m == "name") {
                        xml += "<" + v[m] + ">" + v['value'] + "</" + v[m] + ">";
                    }
                }
            }
        }
        return xml + "";
    }, xml = "";
    for (var m in o) {
        if (o[m].value != "") {
            xml += toXml(o[m], m, "");
        }
    }

    return "<" + rootTag + ">" + xml + "</" + rootTag + ">";

}


function confirmLink(message, url) {
    if (confirm(message)) {
        location.href = url;
    }
    
    return false;
}

function confirmLinkMobile(message, url) {
    if (confirm(message)) {
        $.mobile.changePage(url, { transition: "flip" });
    }

    return false;
}