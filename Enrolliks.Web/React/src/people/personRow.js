const React = require('react');

const PersonRow = props => {
    return (<tr>
        <td>{props.person.name}</td>
    </tr>);
};

module.exports = PersonRow;