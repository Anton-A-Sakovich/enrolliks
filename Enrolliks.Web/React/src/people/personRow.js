const React = require('react');
const ImageButton = require('../imageButton');

module.exports = function PersonRow(props) {
    return (
        <tr>
            <td>
                {props.person.name}
                <ImageButton
                    className='remove-button'
                    src='/img/circle-xmark-regular.svg'
                    hoverSrc='/img/circle-xmark-solid.svg'
                    downSrc='/img/circle-xmark-solid-gray.svg' />
            </td>
        </tr>
    );
};