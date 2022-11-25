const { getPeople } = require('./personEndpoints');
const { default: axios } = require('axios');

module.exports = async function PeopleLoader() {
    const get = url => axios({
        url: url,
        validateStatus: null,
    }).then(response => ({
        status: response.status,
        data: response.data,
    }));

    const result = await getPeople(get);
    return result;
};