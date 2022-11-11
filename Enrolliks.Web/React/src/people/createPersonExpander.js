const React = require('react');
const { createPerson, createPersonResultType } = require('./personEndpoints');
const { validateName } = require('./personUtils');
const axios = require('axios').default;

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
            nameError: validateName(''),
            showNameError: false, // Even if the name error is not empty, it is not shown until the user starts typing.
            requestInProgress: false,
            requestStatus: '',
        };
    }

    render() {
        const errorLabelClass = this.state.showNameError && this.state.nameError.length > 0 ? '' : ' hidden';
        const createButtonDisabled = this.isCreateButtonDisabled();

        return (
            <form onSubmit={event => this.handleSubmit(event)}>
                <div className="panel">
                    <div className='form-row'>
                        <label className='form-label' htmlFor="name">Name</label>
                        <input className='form-input' type="text" id="name" name="name" value={this.state.name} onChange={event => this.handleNameChange(event)} />
                        <label className={'form-error' + errorLabelClass} htmlFor='name'>{this.state.nameError}</label>
                    </div>
                    <div>
                        <button onClick={() => this.handleSubmitClick()} disabled={createButtonDisabled}>Create</button>
                        <button onClick={() => this.handleCancelClick()}>Cancel</button>
                    </div>
                    <div>
                        {this.state.requestStatus}
                    </div>
                </div>
            </form>
        );
    }

    isCreateButtonDisabled() {
        return this.state.nameError.length > 0 || this.state.requestInProgress;
    }

    handleSubmit(event) {
        event.preventDefault();
        return false;
    }

    handleNameChange(event) {
        const name = event.target.value;
        const nameError = validateName(name);

        this.setState({
            name: name,
            nameError: nameError,
            showNameError: true,
        });
    }

    handleCancelClick() {
        this.props.onCancel();
    }

    async handleSubmitClick() {
        if (this.isCreateButtonDisabled())
            return;

        const post = (url, data) => axios({
            url: url,
            method: 'post',
            data: data,
            validateStatus: null,
        }).then(response => {
            return {
                status: response.status,
                data: response.data,
            };
        });

        this.setState({
            requestStatus: 'Sending a request...',
            requestInProgress: true,
        });

        let result;
        try {
            result = await createPerson(post, {
                name: this.state.name,
            });
        } finally {
            this.setState({
                requestInProgress: false,
            })
        }

        switch (result?.tag) {
            case createPersonResultType.success:
                this.props.onComplete();
                break;
            case createPersonResultType.conflict:
                this.setState({requestStatus: 'The person with the name provided already exists.'});
                break;
            case createPersonResultType.validationFailure:
                this.setState({requestStatus: 'The server returned validation error(s).'});
                break;
            case createPersonResultType.badRequest:
                this.setState({requestStatus: 'Something went wrong when sending the request.'});
                break;
            case createPersonResultType.serverError:
                this.setState({requestStatus: 'Something went wrong on the server.'});
                break;
            default:
                this.setState({requestStatus: 'Something went wrong.'});
                break;
        }
    }
}