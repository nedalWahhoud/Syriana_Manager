window.checkScrollEnd = function (dotnetObj, elementId) {

    const element = document.getElementById(elementId);

    if (!element)
        return;

    if (element._scrollHandler) {
        element.removeEventListener('scroll', element._scrollHandler);
    }

    element._scrollHandler = function () {

        console.log(
            element.scrollTop,
            element.clientHeight,
            element.scrollHeight
        );

        if (element.scrollTop + element.clientHeight >= element.scrollHeight - 5) {
            dotnetObj.invokeMethodAsync('OnScrollEnd');
        }
    };

    element.addEventListener('scroll', element._scrollHandler);
};