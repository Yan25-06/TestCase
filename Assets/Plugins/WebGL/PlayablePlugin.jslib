mergeInto(LibraryManager.library, {
    OpenStoreLink: function (url) {
        var urlStr = UTF8ToString(url);
        
        // 1. Kiểm tra môi trường MRAID (Dành cho IronSource, AppLovin, Mintegral)
        if (typeof mraid !== 'undefined') {
            mraid.open(urlStr);
        } 
        // 2. Môi trường dapi (Một số network khác như IronSource có fallback dapi)
        else if (typeof dapi !== 'undefined') {
            dapi.openStoreUrl(urlStr);
        }
        // 3. Mintegral Fallback riêng (nếu MRAID bị lỗi)
        else if (window.install) {
            window.install();
        } 
        // 4. Trình duyệt web bình thường (Fallback khi test trên máy tính)
        else {
            window.open(urlStr, '_blank');
        }
    }
});
