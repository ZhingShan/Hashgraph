@echo off
if not exist "%cd%/.jekyll-cache" mkdir "%cd%/.jekyll-cache
docker run --rm -it --volume="%cd%/docs:/srv/jekyll" --volume="%cd%/.jekyll-cache:/usr/local/bundle" -p 4000:4000 jekyll/jekyll:3.8  jekyll serve --force_polling
rem docker run --rm -it --volume="%cd%/docs:/srv/jekyll" --volume="%cd%/.jekyll-cache:/usr/local/bundle" -p 4000:4000 jekyll/jekyll:3.8  bash