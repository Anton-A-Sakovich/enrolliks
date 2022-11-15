const React = require('react');
const createPersonLogic = require('./createPersonLogic');
const { createPerson: createPersonEndpoint } = require('./personEndpoints');
const { validateName: validatePersonName } = require('./personUtils');
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
                onCancel={this.handleCreatePersonFormCancel}
                onComplete={this.handleCreatePersonFormComplete} />
            : <button onClick={this.handleCreateButtonClick}>Create</button>
    }

    handleCreateButtonClick = () => {
        this.setState({
            editing: true,
        });
    }

    handleCreatePersonFormCancel = () => {
        this.setState({
            editing: false,
        });
    }

    handleCreatePersonFormComplete = () => {
        window.location.reload();
    }
}

const post = (url, personToCreate) => axios({
    url: url,
    method: 'post',
    data: personToCreate,
    validateStatus: null,
}).then(response => ({
    status: response.status,
    data: response.data,
    isHttpProblem: response.headers['content-type'].startsWith('application/problem+json'),
}));

const endpoint = personToCreate => createPersonEndpoint(post, personToCreate);

class CreatePersonForm extends React.Component {
    constructor(props) {
        super(props);
        this.state = createPersonLogic.initialize(validatePersonName);
    }

    render() {
        const errorLabelClass = this.state.showNameError && this.state.nameError.length > 0 ? 'form-error' : 'form-error hidden';
        const createButtonDisabled = this.state.nameError.length > 0 || this.state.requestInProgress;
        const nameInputDisabled = this.state.requestInProgress;

        return (
            <form onSubmit={this.handleSubmit}>
                <div className="panel">
                    <div className='form-row'>
                        <label className='form-label' htmlFor="name">Name</label>
                        <input className='form-input' type="text" id="name" name="name" value={this.state.name} onChange={this.handleNameChange} disabled={nameInputDisabled} />
                        <label className={errorLabelClass} htmlFor='name'>{this.state.nameError}</label>
                    </div>
                    <div>
                        <button onClick={this.handleSubmitClick} disabled={createButtonDisabled}>Create</button>
                        <button onClick={this.handleCancelClick}>Cancel</button>
                    </div>
                    <div>
                        {this.state.requestStatus}
                    </div>
                </div>
            </form>
        );
    }

    handleSubmit = event => {
        event.preventDefault();
        return false;
    };

    handleNameChange = event => {
        this.setState(createPersonLogic.setName(validatePersonName, event.target.value));
    }

    handleCancelClick = () => {
        this.props.onCancel();
    }

    handleSubmitClick = async () => {
        const [stateUpdateBeforeRequest, executeRequest] = createPersonLogic.create(endpoint, this.state);
        
        this.setState(stateUpdateBeforeRequest);
        const stateUpdateAfterRequest = await executeRequest();
        this.setState(stateUpdateAfterRequest);

        if (stateUpdateAfterRequest.requestStatus === '') {
            this.props.onComplete();
        }
    }
}