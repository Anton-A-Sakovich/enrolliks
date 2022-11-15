const React = require('react');
const RemoveButton = require('../removeButton');

module.exports = function PersonRow(props) {
    return (
        <tr>
            <td>{props.person.name} <RemoveButton /></td>
        </tr>
    );
};