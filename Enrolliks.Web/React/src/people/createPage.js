const React = require('react');
const App = require('../app');
const axios = require('axios').default;

class CreatePage extends React.Component {
    constructor(props) {
        super(props);

        this.state = {
            name: '',
            submitAllowed: true,
            error: false
        };
    }

    async submit() {
        try {
            this.setState({submitAllowed: false});

            await axios.post('/api/people/create', {
                name: this.state.name
            });

            window.location.href = window.origin + '/people';
        } catch {
            this.setState({submitAllowed: true, error: true});
        }
    }
    
    render() {
        const errorStyle = {
            display: this.state.error ? 'inline' : 'none'
        };

        const handleNameChange = event => this.setState({name: event.target.value});
        const handleButtonClick = () => this.submit();

        return (
            <div>
                <label>Name</label>
                <span style={errorStyle}>Error</span>
                <input type={'text'} value={this.state.name} onChange={handleNameChange}></input>
                <button onClick={handleButtonClick} disabled={!this.state.submitAllowed}>Create</button>
            </div>
        );
    }
}

App.definePage(CreatePage);
module.exports = CreatePage;