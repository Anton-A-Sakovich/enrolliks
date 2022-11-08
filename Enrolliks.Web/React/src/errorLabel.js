const React = require('react');

module.exports = function ErrorLabel(props) {
    const errorMessage = props.children;
    const showError = props.showError;

    const display = showError && errorMessage.length > 0 ? 'inline' : 'none';

    return <label style={{display: display}} {...props}>{errorMessage}</label>;
}