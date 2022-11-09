const React = require('react');
const person = require('./person');

module.exports = class CreatePersonExpander extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            editing: false,
        };
    }

    render() {
        return this.state.editing
            ? <CreatePersonForm
                onCancel={() => this.setState({editing: false})}
                onComplete={() => this.setState({editing: false})} />
            : <button onClick={() => this.setState({editing: true})}>Create</button>
    }
}

class CreatePersonForm extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            name: '',
            nameError: person.validateName(''),
            showNameError: false,
        };
    }

    render() {
        const hidden = this.state.showNameError && this.state.nameError.length > 0 ? '' : ' hidden';

        return (
            <form onSubmit={event => this.onSubmit(event)}>
                <div className="panel">
                    <div className='form-row'>
                        <label className='form-label' htmlFor="name">Name</label>
                        <input className='form-input' type="text" id="name" name="name" value={this.state.name} onChange={event => this.handleNameChange(event)} />
                        <label className={'form-error' + hidden} htmlFor='name'>{this.state.nameError}</label>
                    </div>
                    <div>
                        <button onClick={() => this.handleSubmitClick()} disabled={this.state.nameError.length > 0}>Create</button>
                        <button onClick={() => this.handleCancelClick()}>Cancel</button>
                    </div>
                </div>
            </form>
        );
    }

    onSubmit(event) {
        event.preventDefault();
        return false;
    }

    handleNameChange(event) {
        const name = event.target.value;
        const nameError = person.validateName(name);

        this.setState({
            name: name,
            nameError: nameError,
            showNameError: true,
        });
    }

    handleCancelClick() {
        this.props.onCancel();
    }

    handleSubmitClick() {
        this.props.onComplete();
    }
}