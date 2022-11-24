const { useState } = require('react');
const React = require('react');

function ImageButton(props) {
    const [isMouseInside, setIsMouseInside] = useState(false);
    const [isMouseDown, setIsMouseDown] = useState(false);
    const {inputRef, src, hoverSrc, downSrc, ...restProps} = props;

    let [handleMouseEnter, handleMouseLeave] = [undefined, undefined];
    if (hoverSrc || downSrc) {
        handleMouseEnter = () => setIsMouseInside(true);
        handleMouseLeave = () => setIsMouseInside(false);
    }

    let [handleMouseDown, handleMouseUp] = [undefined, undefined];
    if (downSrc) {
        handleMouseDown = () => setIsMouseDown(true);
        handleMouseUp = () => setIsMouseDown(false);
    }

    let currentSource;
    if (isMouseInside && isMouseDown)
        currentSource = downSrc || hoverSrc || src;
    else if (isMouseInside)
        currentSource = hoverSrc || src;
    else
        currentSource = src;

    return (
        <button
            ref={inputRef}
            onMouseEnter={handleMouseEnter}
            onMouseLeave={handleMouseLeave}
            onMouseDown={handleMouseDown}
            onMouseUp={handleMouseUp}
            {...restProps}>
            <img src={currentSource} />
        </button>
    );
}

function ImageButtonForwarder(props, ref) {
    return <ImageButton inputRef={ref} {...props} />;
}

module.exports = React.forwardRef(ImageButtonForwarder);