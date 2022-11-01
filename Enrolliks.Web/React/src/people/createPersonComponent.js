const React = require('react');

class CreatePersonComponent extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            editing: false,
            name: '',
        };
    }

    render() {
        if (!this.state.editing) {
            return <button onClick={() => this.handleCreateClick()}>Create</button>;
        } else {
            return (
            <form onSubmit={event => this.onSubmit(event)}>
                <div className="panel">
                    <div className='form-row'>
                        <label className='form-label' htmlFor="name">Name</label>
                        <input className='form-input' type="text" id="name" name="name" value={this.state.name} onChange={event => this.handleNameChange(event)} />
                    </div>
                    <div>
                        <button onClick={() => this.handleSubmitClick()}>Create</button>
                        <button onClick={() => this.handleCancelClick()}>Cancel</button>
                    </div>
                </div>
            </form>);
        }
    }

    onSubmit(event) {
        event.preventDefault();
        return false;
    }

    handleNameChange(event) {
        this.setState({
            name: event.target.value,
        });
    }

    handleCreateClick() {
        this.setState({
            editing: true,
        });
    }

    handleCancelClick() {
        this.setState({
            editing: false,
            name: '',
        });
    }

    handleSubmitClick() {
    }
}

module.exports = CreatePersonComponent;