const React = require('react');

module.exports = function PersonRow(props) {
    return (
        <tr>
            <td>{props.person.name}</td>
        </tr>
    );
};