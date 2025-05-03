window.registerCompositionListener = (dotNetHelper) => {
    const inputField = document.getElementById('inputField');
    let composing = false;
    let kanaLocked = false;
    let lastKana = "";
    if (inputField == null) return;
    inputField.addEventListener('compositionstart', (e) => {
        composing = true;
        kanaLocked = false;
        dotNetHelper.invokeMethodAsync('CompositionStart');
    });

    inputField.addEventListener('compositionupdate', (e) => {
        if (composing) {
            lastKana = e.data;
            dotNetHelper.invokeMethodAsync('UpdateKana', e.data, composing);
        }
    });

    inputField.addEventListener('compositionend', (e) => {
        composing = false;
        kanaLocked = true;
        dotNetHelper.invokeMethodAsync('CompositionEnd');
        //inputField.value = lastKana; // 確定時にカナで上書き
    });

    inputField.addEventListener('input', (e) => {
        if (!composing) {
            // 日本語入力モードではない通常input（英数とか）だけ拾う
            dotNetHelper.invokeMethodAsync('UpdateKana', e.target.value, composing);
        }
    });
};
