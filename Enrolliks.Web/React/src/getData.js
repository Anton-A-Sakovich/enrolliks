module.exports = function getData() {
    if (!window.data)
        return null;

    return window.data;
}