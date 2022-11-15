const { useState } = require('react');
const React = require('react');

function RemoveButton(props) {
    const [isHover, setIsHover] = useState(false);
    const {inputRef, ...restProps} = props;

    return (
        <button
            className='remove-button'
            onMouseEnter={() => setIsHover(true)}
            onMouseLeave={() => setIsHover(false)}
            ref={inputRef}
            {...restProps}>
            <img src={isHover ? '/img/circle-xmark-solid.svg' : '/img/circle-xmark-regular.svg'} />
        </button>
    )
}

function RemoveButtonForwarder(props, ref) {
    return <RemoveButton inputRef={ref} {...props} />;
}

module.exports = React.forwardRef(RemoveButtonForwarder);