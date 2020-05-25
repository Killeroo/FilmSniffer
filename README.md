# FilmSniffer
Commandline film lookup tool, information is fetched via the [OMDb](http://www.omdbapi.com) API. 

![alt text](https://user-images.githubusercontent.com/9999745/82820712-9464af00-9e9a-11ea-8763-168d86317281.png "")

## How to use

Place films in a text file called Films.txt in program directory or specify another location using ```/path``` command. Each film should be on it's own line in the text file.

**Note:** In order to use the tool you will need an OMDb API key, you can get a free key [here](http://www.omdbapi.com/apikey.aspx).


## Downloads
You can download the latest release [here](https://github.com/Killeroo/FilmSniffer/releases). 

## Arguments
```
  /key            <string>  - (REQUIRED) OMDb API key (You can request a key here http://www.omdbapi.com/apikey.aspx)
  /path           <string>  - Path to films file (one film on each line)
  /seperatefiles            - Save some film stats to seperate CSV files"
  /help                     - Displays this help text
```

## License

License under the MIT License:

Copyright (c) 2020 Matthew Carney <matthewcarney64@gmail.com>